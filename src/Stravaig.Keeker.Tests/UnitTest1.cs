using System;
using System.IO;
using NUnit.Framework;
using Shouldly;

namespace Stravaig.Keeker.Tests;

public class Tests : IDisposable
{
    private readonly AssemblyBuilder _builder;
    private readonly IsolatedAssembly _assembly;
    private readonly string _assemblyPath;

    public Tests()
    {
        _builder = new AssemblyBuilder();
        _assemblyPath = _builder.Build("SimpleClassLibrary");
        _assembly = IsolatedAssembly.Load(_assemblyPath);
    }
    
    [Test]
    public void AssemblyReportsCorrectLocation()
    {
        _assembly.Location.ShouldBe(_assemblyPath);
    }

    public void Dispose()
    {
        _assembly.Dispose();
        _builder.Tidy();
    }
}