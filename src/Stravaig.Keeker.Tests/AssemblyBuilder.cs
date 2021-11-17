using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Stravaig.Keeker.Tests;

public class AssemblyBuilder
{
    private const int BuildTimeoutMs = 30000;
    private static readonly object SyncRoot = new ();
    private static readonly string BaseRunId;
    private static int _counter;

    private readonly string _runId;
    private string _outputDirectory;

    static AssemblyBuilder()
    {
        BaseRunId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
    }
    
    public AssemblyBuilder()
    {
        lock (SyncRoot)
        {
            _runId = BaseRunId + "-" + _counter++;
        }
    }

    public string Build(string projectName)
    {
        return BuildImpl(projectName);
    }

    public void Tidy()
    {
        Directory.Delete(_outputDirectory, true);
        
    }
    
    private string BuildImpl(string projectName, [CallerFilePath]string? filePath = null)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        
        _outputDirectory = GetTempDirectoryPath(projectName);
        Console.WriteLine("Temp Directory: "+_outputDirectory);
        var projectPath = GetProjectPath(projectName, filePath);
        Console.WriteLine("Project Path: "+projectPath);

        var process = RunBuildProcess(projectPath, _outputDirectory);
        CheckBuildForErrors(process);
        return FindAssemblyFile(projectName, _outputDirectory);
    }

    private static string FindAssemblyFile(string projectName, string tempDirectory)
    {
        FileInfo assemblyFile = new FileInfo(Path.Join(tempDirectory, projectName + ".dll"));
        if (assemblyFile.Exists)
            return assemblyFile.FullName;

        throw new InvalidOperationException(
            "Build process succeeded, but assembly not found at " +
            assemblyFile.FullName);
    }

    private static void CheckBuildForErrors(Process process)
    {
        if (process.ExitCode != 0)
        {
            var stdout = process.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(stdout))
            {
                Console.WriteLine("Standard Out:");
                Console.WriteLine(stdout);
            }

            var stderr = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                Console.WriteLine();
                Console.WriteLine("Standard Error:");
                Console.WriteLine(stderr);
                throw new InvalidOperationException("The process failed." + Environment.NewLine + stderr);
            }

            throw new InvalidOperationException("The process failed.");
        }
    }

    private static Process RunBuildProcess(string projectPath, string tempDirectory)
    {
        var processInfo = new ProcessStartInfo("dotnet")
        {
            ArgumentList =
            {
                "build",
                projectPath,
                "--output",
                tempDirectory,
            },
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ErrorDialog = false,
        };
        var process = Process.Start(processInfo);

        if (process == null)
            throw new InvalidOperationException("Build process failed to start.");

        process.WaitForExit(BuildTimeoutMs);
        return process;
    }

    private string GetProjectPath(string projectName, string filePath)
    {
        var srcDirectory = new FileInfo(filePath).Directory?.Parent?.FullName;
        if (srcDirectory == null)
            throw new InvalidOperationException($"Cannot find the source directory from: \"{filePath}\"");

        var projectPath = Path.Join(srcDirectory, projectName, projectName + ".csproj");

        return projectPath;
    }

    private string GetTempDirectoryPath(string projectName)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        var generalDirectory = Path.Join(Path.GetTempPath(), assemblyName);
        Directory.CreateDirectory(generalDirectory);

        var testAssemblyDirectory = Path.Join(generalDirectory, projectName);
        Directory.CreateDirectory(testAssemblyDirectory);

        var runDirectory = Path.Join(testAssemblyDirectory, _runId);
        Directory.CreateDirectory(runDirectory);

        return runDirectory;
    }
}