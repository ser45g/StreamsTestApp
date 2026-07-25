using StreamsTestApp.Extensions;
using System.Text;

Console.WriteLine("Download a file at: ");

var url = Console.ReadLine();

if (string.IsNullOrWhiteSpace(url))
{
    Console.WriteLine("Please, enter url");
    return;
}

string? defaultDownloadFolderPath = null;
if (OperatingSystem.IsWindows())
{
    defaultDownloadFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
}
Console.WriteLine($"To what directory you want it to be saved({defaultDownloadFolderPath??""}):");

var path = Console.ReadLine();

if (string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(defaultDownloadFolderPath))
{
    path = defaultDownloadFolderPath;
} 

if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
{
    Console.WriteLine("No such directory, try again.");
    return;
}

Console.WriteLine($"Saving to: {path}");

try
{
    Console.WriteLine("Start downloading...");

    using var httpClient = new HttpClient();

    using var response = await httpClient.GetAsync(url);

    var contentLenght = response.Content.Headers.ContentLength;

    var contentDispositionFileName = response.Content.Headers?.ContentDisposition?.FileName;

    var urlFileName = new Uri(url).Segments.LastOrDefault();

    string fileName = contentDispositionFileName ?? urlFileName ?? $"downlaoded-file-{Guid.NewGuid()}";

    await using var responseStream = await response.Content.ReadAsStreamAsync();

    using var fileStream = File.Open(Path.Combine(path, fileName), FileMode.OpenOrCreate);

    long totalBytesRead = 0;
    await responseStream.CopyToStreamWithProgressAsync(fileStream, bufferSize:8*1024*8, bytesReadProgressCallback: (long bytesRead) => {

        totalBytesRead += bytesRead;
        StringBuilder stringBuilder = new();

        stringBuilder.Append($"Read: {bytesRead} bytes. ");

        if (contentLenght != null)
        {   
            stringBuilder.Append($"Downloaded: {((double)totalBytesRead / contentLenght * 100):F2}%");
        }

        Console.WriteLine(stringBuilder.ToString());
    });

    Console.WriteLine($"Downloading finished! The file is at: {Path.Combine(path, fileName)}");
}
catch(Exception ex)
{
    Console.WriteLine("Couldn't download a file!");
}





