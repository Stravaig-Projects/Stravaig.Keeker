using System.IO.Compression;

namespace Stravaig.Keeker;

public class Package
{
    private Package(string unzippedDirectory, string packagePath)
    {
        PackagePath = packagePath;
        UnzippedDirectory = unzippedDirectory;
    }
    
    public string PackagePath { get; }
    
    public string UnzippedDirectory { get; }
    
    // e.g. https://www.nuget.org/api/v2/package/Stravaig.Extensions.Logging.Diagnostics/1.2.0-preview.212
    public static async Task<Package> LoadAsync(Uri uri, TempDirectory tempDirectory, string packageName, CancellationToken ct)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(uri, ct);

        // TODO: Handle non-200 responses

        string directoryPath = tempDirectory.CreateTempDirectory(TempDirectoryType.NuGetPackage, packageName);
        var filePath = await WritePackageAsync(response, directoryPath, packageName, ct);

        await using var packageStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var zip = new ZipArchive(packageStream, ZipArchiveMode.Read);
        foreach (var entry in zip.Entries)
        {
            var path = Path.Combine(directoryPath, entry.FullName);
            entry.ExtractToFile(path, true);
        }

        return new Package(directoryPath, filePath);
    }

    private static async Task<string> WritePackageAsync(
        HttpResponseMessage response,
        string directoryPath,
        string packageName,
        CancellationToken ct)
    {
        string filePath = Path.Combine(directoryPath, packageName);
        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await response.Content.CopyToAsync(fs, ct);
        return filePath;
    }
}