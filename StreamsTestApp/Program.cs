

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

    await using var responseStream = await httpClient.GetStreamAsync(url);

    using var fileStream = File.Open(Path.Combine(path, "downloaded-file"), FileMode.OpenOrCreate);

    await responseStream.CopyToAsync(fileStream);

    Console.WriteLine("Downloading finished!");
}
catch(Exception ex)
{
    Console.WriteLine("Couldn't download a file!");
}





