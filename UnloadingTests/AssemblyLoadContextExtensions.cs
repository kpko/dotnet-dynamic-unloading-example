using System.Reflection;
using System.Runtime.Loader;

namespace UnloadingTests;

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