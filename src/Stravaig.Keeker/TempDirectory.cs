using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stravaig.Keeker;

public enum TempDirectoryType
{
    NuGetPackage,
    Project,
}

/// <summary>
/// Used to create Temp directories in the format /tmp/Stravaig.Keeker/[type]-[name]/[date]-[run]/
/// </summary>
public class TempDirectory : IDisposable
{
    private readonly ILogger<TempDirectory> _logger;
    
    private static readonly object SyncRoot = new ();
    private static readonly string BaseRunId;
    private static int _counter;
    
    private ConcurrentBag<string> _paths = new ();
    
    static TempDirectory()
    {
        BaseRunId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
    }
    
    public TempDirectory(ILogger<TempDirectory> logger)
    {
        _logger = logger;
    }

    public TempDirectory()
    {
        _logger = new NullLogger<TempDirectory>();
    }
    
    public string CreateTempDirectory(TempDirectoryType type, string name)
    {
        var generalDirectory = GetBaseTempDirectory();
        var baseRunDirectory = GetBaseItemDirectory(generalDirectory, type, name);
        var runDirectory = GetRunDirectory(baseRunDirectory);

        _paths.Add(runDirectory);
        
        return runDirectory;
    }

    private static string GetRunDirectory(string baseDirectory)
    {
        string runDirectoryName;
        lock (SyncRoot)
        {
            runDirectoryName = $"{BaseRunId}-{_counter++}";
        }
        var runDirectory = Path.Join(baseDirectory, runDirectoryName);
        
        if (!Directory.Exists(runDirectory))
            Directory.CreateDirectory(runDirectory);
        return runDirectory;
    }

    private static string GetBaseItemDirectory(string baseDirectory, TempDirectoryType type, string name)
    {
        var baseRunDirectoryName = $"{type}-{name}";
        var baseRunDirectory = Path.Join(baseDirectory, baseRunDirectoryName);
        
        if (!Directory.Exists(baseRunDirectory))
            Directory.CreateDirectory(baseRunDirectory);
        return baseRunDirectory;
    }

    private static string GetBaseTempDirectory()
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        var generalDirectory = Path.Join(Path.GetTempPath(), assemblyName);
        if (!Directory.Exists(generalDirectory))
            Directory.CreateDirectory(generalDirectory);
        return generalDirectory;
    }

    private void ReleaseUnmanagedResources()
    {
        foreach (var path in _paths)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (DirectoryNotFoundException dnfEx)
            {
                _logger.LogDebug(
                    exception: dnfEx,
                    message: "The directory ({Path}) was not found.",
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(exception: ex, message: "Unable to remove the temp directory {Path}.", path);
            }
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~TempDirectory()
    {
        ReleaseUnmanagedResources();
    }
}