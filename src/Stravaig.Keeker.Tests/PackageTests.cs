using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Stravaig.Extensions.Logging.Diagnostics;

namespace Stravaig.Keeker.Tests;

[TestFixture]
public class PackageTests
{
    [Test]
    public async Task AllFilesAccountedFor()
    {
        var logger = new TestCaptureLogger<TempDirectory>();
        using var tempDirectory = new TempDirectory(logger);
        var uri = new Uri("https://www.nuget.org/api/v2/package/Stravaig.Extensions.Logging.Diagnostics/1.2.0-preview.212");
        var package = await Package.LoadAsync(uri, tempDirectory, "Stravaig.Extensions.Logging.Diagnostics-1.2.0-preview.212.nupkg", CancellationToken.None);

        Console.WriteLine("Package path: "+package.PackagePath);
        File.Exists(package.PackagePath).ShouldBeTrue();

        Console.WriteLine("Unzipped Directory = "+package.UnzippedDirectory);
        Directory.Exists(package.UnzippedDirectory).ShouldBeTrue();

        foreach (var file in Directory.EnumerateFiles(package.UnzippedDirectory, "*", SearchOption.AllDirectories))
        {
            Console.WriteLine(file);
        }
    }
}