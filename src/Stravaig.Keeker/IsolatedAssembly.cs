using System.Reflection;
using System.Runtime.Loader;

namespace Stravaig.Keeker;

public class IsolatedAssembly : IDisposable
{
    private readonly Assembly _assembly;
    private readonly AssemblyLoadContext _context;

    private IsolatedAssembly(AssemblyLoadContext context, Assembly assembly)
    {
        _context = context;
        _assembly = assembly;
    }

    public static IsolatedAssembly Load(string path)
    {
        var context = new AssemblyLoadContext(null, true);
        var assembly = context.LoadFromAssemblyPath(path);
        return new IsolatedAssembly(context, assembly);
    }

    private void ReleaseUnmanagedResources()
    {
        _context.Unload();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~IsolatedAssembly()
    {
        ReleaseUnmanagedResources();
    }
    
    public string? Name => _assembly.FullName;

    public string Location => _assembly.Location;
}