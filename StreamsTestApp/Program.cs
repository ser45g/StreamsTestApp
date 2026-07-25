using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;
using StreamsTestApp.Commands;
using StreamsTestApp.Helpers;

IServiceCollection services = null!;

IHost _host = Host.CreateDefaultBuilder().ConfigureServices((hostContext, s) =>
{
    //register services here

    services = s;
}).Build();

var registrar = new DITypeRegistar(services);

var app = new CommandApp(registrar);

app.SetDefaultCommand<DownloadFileCommand>();

app.Configure(config =>
{
    config.SetApplicationName("file-downloader");
    config.ValidateExamples();
    config.AddExample("file", "download");

    config.AddBranch<CommandSettings>("file", fileBranch =>
    {
        fileBranch.SetDescription("Working with files");
        fileBranch.AddCommand<DownloadFileCommand>("download");
    });

    
    app.Configure(config =>
    {
        config.SetExceptionHandler((ex, resolver) =>
        {
            IAnsiConsole? _ansiConsole = (IAnsiConsole)resolver.Resolve(typeof(IAnsiConsole));
            _ansiConsole?.WriteException(ex, ExceptionFormats.NoStackTrace | ExceptionFormats.ShortenEverything);

            return 1;
        });
    });


});

// Create a cancellation token source to handle Ctrl+C
var cancellationTokenSource = new CancellationTokenSource();

// Wire up Console.CancelKeyPress to trigger cancellation
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // Prevent immediate process termination
    cancellationTokenSource.Cancel();
    Console.WriteLine("Cancellation requested...");
};

_host.Start();

await app.RunAsync(args, cancellationToken: cancellationTokenSource.Token);

