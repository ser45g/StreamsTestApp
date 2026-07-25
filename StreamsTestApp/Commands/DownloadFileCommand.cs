using Spectre.Console;
using Spectre.Console.Cli;
using StreamsTestApp.Extensions;
using System.ComponentModel;
using System.Net.Http.Headers;


namespace StreamsTestApp.Commands
{
    [Description("Download a file by its url.")]
    public sealed class DownloadFileCommand : AsyncCommand
    {
        private readonly IAnsiConsole _ansiConsole;

        private readonly IHttpClientFactory _httpClientFactory;

        public DownloadFileCommand(IAnsiConsole ansiConsole, IHttpClientFactory httpClientFactory)
        {
            _ansiConsole = ansiConsole;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var url = _ansiConsole.Prompt(new TextPrompt<string>("Download a file at: "));

            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("Please, enter url");
                return -1;
            }

            string? defaultDownloadFolderPath = GetDefaultDownloadFolderForCurrentOS();
           
            var path = _ansiConsole.Prompt(new TextPrompt<string?>("To what directory you want it to be saved?").DefaultValue(defaultDownloadFolderPath));

            if (string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(defaultDownloadFolderPath))
            {
                path = defaultDownloadFolderPath;
            }

            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                _ansiConsole.WriteLine("No such directory, try again.");
                return -1;
            }

            _ansiConsole.WriteLine($"Saving to: {path}");

            try
            {
                _ansiConsole.WriteLine("Start downloading...");

                var filePath = await DownloadFile(url, path, cancellationToken);

                _ansiConsole.WriteLine($"Downloading finished! The file is at: {filePath}");
                
            }
            catch (Exception ex)
            {
                _ansiConsole.WriteLine("Couldn't download a file!");
                return -1;
            }

            return 0;
        }

        private string? GetDefaultDownloadFolderForCurrentOS()
        {
            string? defaultDownloadFolderPath = null;

            if (OperatingSystem.IsWindows())
            {
                defaultDownloadFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }

            return defaultDownloadFolderPath;
        }

        private string GetFileName(string url, HttpContentHeaders contentHeaders)
        {
            var contentDispositionFileName = contentHeaders?.ContentDisposition?.FileName;

            var urlFileName = new Uri(url).Segments.LastOrDefault();

            return contentDispositionFileName ?? urlFileName ?? $"downlaoded-file-{Guid.NewGuid()}";
        }

        private async Task<string> DownloadFile(string url, string path, CancellationToken cancellationToken = default)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            using var response = await httpClient.GetAsync(url, cancellationToken);

            var contentLenght = response.Content.Headers.ContentLength;

            var fileName = GetFileName(url, response.Content.Headers);

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            using var fileStream = File.Open(Path.Combine(path, fileName), FileMode.OpenOrCreate);

            if (contentLenght != null)
            {
                long totalBytesRead = 0;

                await _ansiConsole.Progress().StartAsync(async (ctx) =>
                {
                    var task = ctx.AddTask("Downloading a file", maxValue: 100);

                    await responseStream.CopyToStreamWithProgressAsync(fileStream, bufferSize: 8 * 1024 * 8, bytesReadProgressCallback: (long bytesRead) => {

                        totalBytesRead += bytesRead;

                        double percent = 100 * totalBytesRead / (double)contentLenght;
                        task.Value(percent);

                    }, cancellationToken: cancellationToken);

                    task.Value = 100;
                    task.StopTask();
                });
            }
            else
            {
                await _ansiConsole.Status().StartAsync("Downloading a file", async (ctx) =>
                {
                    ctx.Spinner(Spinner.Known.Star2);

                    await responseStream.CopyToStreamWithProgressAsync(fileStream, bufferSize: 8 * 1024 * 8, cancellationToken: cancellationToken);
                });
            }

            return Path.Combine(path, fileName);
        }

    }
}
