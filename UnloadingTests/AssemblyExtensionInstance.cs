using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

using Microsoft.Extensions.DependencyInjection;

using Shared;

namespace UnloadingTests;

public class AssemblyExtensionInstance : IDisposable
{
    public WeakReference Ref => _ctxRef;
    public bool IsLoaded => _ctxRef.IsAlive;
    private AssemblyLoadContext _ctx = null!;
    private WeakReference _ctxRef = null!;

    private Dictionary<string, IActivity> _activites = new();
    private IServiceProvider? _provider;

    private AssemblyExtensionInstance()
    {
    }

    [MemberNotNull(nameof(_ctxRef), nameof(_ctx), nameof(_provider))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static AssemblyExtensionInstance LoadFromAssembly(string path)
    {
        var assemblyPath = Path.GetFullPath(path);

        var instance = new AssemblyExtensionInstance();
        var alc = new ExtensionAssemblyLoadContext("test", assemblyPath);
        var assembly = alc.LoadFromAssemblyPath(assemblyPath);
        instance._ctxRef = new WeakReference(alc, trackResurrection: true);
        instance._ctx = alc;

        var setupType = assembly.GetExportedTypes().SingleOrDefault(t => t.IsAssignableTo(typeof(IExtensionSetup)));
        if (setupType == null) return instance;

        var setup = (IExtensionSetup)Activator.CreateInstance(setupType)!;
        instance._provider = setup.GetProvider();

        var activities = instance._provider.GetRequiredService<IEnumerable<IActivity>>();
        foreach (var activity in activities)
        {
            instance._activites.Add(activity.GetType().Name, activity);
        }

        return instance;
    }

    public void CallTestMethod(string key, Context context)
    {
        if (_activites.TryGetValue(key, out var activity))
        {
            activity.Execute(context);
        }
    }

    public void Dispose()
    {
        Unload();
    }

    public void Unload()
    {
        _activites.Clear();
        _activites = null!;

        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _provider = null!;
        _ctx = null!;

        if (_ctxRef.Target is ExtensionAssemblyLoadContext alc)
        {
            alc.Unload();
        }
    }
}