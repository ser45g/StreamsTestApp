using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;
using StreamsTestApp.Commands;
using StreamsTestApp.Helpers;

IServiceCollection serviceCollection = null!;

IHost _host = Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) =>
{
    //register services here
    services.AddHttpClient();

    serviceCollection = services;
}).Build();

var registrar = new DITypeRegistar(serviceCollection);

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
            object? ansiConsoleObject = resolver?.Resolve(typeof(IAnsiConsole));

            var ansiConsole = ansiConsoleObject as IAnsiConsole;

            ansiConsole?.WriteException(ex, ExceptionFormats.NoStackTrace | ExceptionFormats.ShortenEverything);

            return 1;
        });
    });
});

var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; 
    cancellationTokenSource.Cancel();
    Console.WriteLine("Cancellation requested...");
};

_host.Start();

await app.RunAsync(args, cancellationToken: cancellationTokenSource.Token);

