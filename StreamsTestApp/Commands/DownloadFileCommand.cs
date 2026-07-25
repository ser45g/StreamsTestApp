using Spectre.Console;
using Spectre.Console.Cli;
using StreamsTestApp.Extensions;
using System.ComponentModel;
using System.Text;


namespace StreamsTestApp.Commands
{
    [Description("Download a file by its url.")]
    public sealed class DownloadFileCommand : AsyncCommand
    {
        private readonly IAnsiConsole _ansiConsole;

        public DownloadFileCommand(IAnsiConsole ansiConsole)
        {
            _ansiConsole = ansiConsole;
        }

        protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            Console.WriteLine();

            var url = _ansiConsole.Prompt(new TextPrompt<string>("Download a file at: "));

            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("Please, enter url");
                return -1;
            }

            string? defaultDownloadFolderPath = null;
            if (OperatingSystem.IsWindows())
            {
                defaultDownloadFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
           
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

                using var httpClient = new HttpClient();

                using var response = await httpClient.GetAsync(url);

                var contentLenght = response.Content.Headers.ContentLength;

                var contentDispositionFileName = response.Content.Headers?.ContentDisposition?.FileName;

                var urlFileName = new Uri(url).Segments.LastOrDefault();

                string fileName = contentDispositionFileName ?? urlFileName ?? $"downlaoded-file-{Guid.NewGuid()}";

                await using var responseStream = await response.Content.ReadAsStreamAsync();

                using var fileStream = File.Open(Path.Combine(path, fileName), FileMode.OpenOrCreate);

                long totalBytesRead = 0;
                await responseStream.CopyToStreamWithProgressAsync(fileStream, bufferSize: 8 * 1024 * 8, bytesReadProgressCallback: (long bytesRead) => {

                    totalBytesRead += bytesRead;
                    StringBuilder stringBuilder = new();

                    stringBuilder.Append($"Read: {bytesRead} bytes. ");

                    if (contentLenght != null)
                    {
                        stringBuilder.Append($"Downloaded: {((double)totalBytesRead / contentLenght * 100):F2}%");
                    }

                    _ansiConsole.WriteLine(stringBuilder.ToString());
                });

                _ansiConsole.WriteLine($"Downloading finished! The file is at: {Path.Combine(path, fileName)}");
                
            }
            catch (Exception ex)
            {
                _ansiConsole.WriteLine("Couldn't download a file!");
                return -1;
            }

            return 0;
        }
    }
}
