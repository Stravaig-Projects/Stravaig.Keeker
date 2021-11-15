namespace Stravaig.Keeker;

public class NamespaceFacade
{
    private readonly IsolatedAssembly _isolatedAssembly;
    private readonly string _namespace;

    public NamespaceFacade(IsolatedAssembly isolatedAssembly, string @namespace)
    {
        _isolatedAssembly = isolatedAssembly;
        _namespace = @namespace;
    }

    public string Name => _namespace;
    
    

}