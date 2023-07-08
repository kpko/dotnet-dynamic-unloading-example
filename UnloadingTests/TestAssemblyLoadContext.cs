using System.Reflection;
using System.Runtime.Loader;

using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace UnloadingTests;

class ExtensionAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public ExtensionAssemblyLoadContext(string name, string mainAssemblyToLoadPath) : base(name: name, isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
    }

    protected override Assembly? Load(AssemblyName name)
    {
        if (AssemblyLoadContext.Default.TryLoadFromAssemblyName(name, out var a))
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
public static class AssemblyLoadContextExtensions
{
    public static bool TryLoadFromAssemblyName(this AssemblyLoadContext ctx, AssemblyName name, out Assembly? assembly)
    {
        try
        {
            assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(name);
            return true;
        }
        catch (Exception)
        {
            assembly = null;
            return false;
        }
    }
}