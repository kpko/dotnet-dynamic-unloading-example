using System.Reflection;
using System.Runtime.Loader;

namespace UnloadingTests;

public class ExtensionAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public ExtensionAssemblyLoadContext(string name, string mainAssemblyToLoadPath) : base(name: name, isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
    }

    protected override Assembly? Load(AssemblyName name)
    {
        // Try to load from default context first. This is important for shared assemblies.
        if (Default.TryLoadFromAssemblyName(name, out var a))
        {
            return a;
        }

        string? assemblyPath = _resolver.ResolveAssemblyToPath(name);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }
}