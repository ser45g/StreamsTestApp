using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Scalar.AspNetCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseFileServer(enableDirectoryBrowsing: true);

app.MapGet("/file/{filePath}", ([FromRoute] string filePath, IWebHostEnvironment environment) =>
{
    try
    {
        filePath = Uri.UnescapeDataString(filePath);

        filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
        
        string absoluteFilePath = Path.Combine(environment.WebRootPath, filePath);

        var directoryForFile = Directory.GetParent(absoluteFilePath)?.FullName;

        if(directoryForFile == null || !File.Exists(absoluteFilePath))
        {
            return Results.NotFound();
        }

        if (!directoryForFile.StartsWith(environment.WebRootPath))
        {
            return Results.StatusCode((int)HttpStatusCode.Forbidden);
        }

        var provider = new FileExtensionContentTypeProvider();

        provider.TryGetContentType(absoluteFilePath, out string? contentType);

        var fileStream = File.OpenRead(absoluteFilePath);

        return Results.File(fileStream, 
            fileDownloadName: Path.GetFileName(absoluteFilePath), 
            contentType: contentType);
    }
    catch(Exception ex)
    {
        return Results.BadRequest("Could not serve this file.");
    }
});

app.Run();
