
using StreamsTestApp.Extensions;

Console.WriteLine("Download a file at: ");

var url = Console.ReadLine();

Console.WriteLine("Save it to directory:");
var path = Console.ReadLine();

if (path==null || !Directory.Exists(path))
{
    Console.WriteLine("No such directory, try again.");
    return;
}

try
{
    Console.WriteLine("Start downloading...");

    HttpClient httpClient = new HttpClient();

    using var response = await httpClient.GetAsync(url);

    var contentLenght = response.Content.Headers.ContentLength;

    await using var responseStream = await response.Content.ReadAsStreamAsync();

    using var fileStream = File.Open(Path.Combine(path, "downloaded-file"), FileMode.OpenOrCreate);

    long totalBytesRead = 0;
    await responseStream.CopyToStreamWithProgressAsync(fileStream, (long bytesRead) => {

        totalBytesRead += bytesRead;
        Console.WriteLine($"Read: {bytesRead} bytes.");

        if (contentLenght != null)
        {
            Console.WriteLine($"Downloaded: {((double)totalBytesRead / contentLenght * 100):F2}%");
        }
    });

    Console.WriteLine("Downloading finished!");
}
catch(Exception ex)
{
    Console.WriteLine("Couldn't download a file!");
}





