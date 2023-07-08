using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

using Microsoft.Extensions.DependencyInjection;

using Shared;

using UnloadingTests;


var path = Path.GetFullPath("../../../../Lib/bin/Release/net7.0/publish/Lib.dll");
var instance = AssemblyExtensionInstance.LoadFromAssembly(path);

Console.WriteLine(instance.IsLoaded);

var ctx = new Context();

instance.CallTestMethod("Test", ctx);
instance.CallTestMethod("Test2", ctx);

Console.WriteLine(instance.IsLoaded);


// Try clean up the ref for at least 10 times
for (var i = 0; instance.IsLoaded && (i < 10); i++)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
}

instance.CallTestMethod("Test2", ctx);

Console.ReadKey();

instance.Unload();

// Try clean up the ref for at least 10 times
for (var i = 0; instance.IsLoaded && (i < 10); i++)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
}

Console.WriteLine(instance.IsLoaded);

foreach (var (key, value) in ctx.Stuff)
{
    Console.WriteLine($"{key}: {value}");
}


internal class AssemblyExtensionInstance : IDisposable
{
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

        // var args = new object[1] { new string[] { "Hello" } };
        // _ = a.EntryPoint?.Invoke(null, args);

        // var types = assembly.ExportedTypes
        //     .Where(t => t.IsAssignableTo(typeof(IActivity)))
        //     .ToList();
        // foreach (var type in types)
        // {
        //     var obj = (IActivity)Activator.CreateInstance(type)!;
        //     instance._activites.Add(type.Name, obj);
        // }

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
        // foreach (var (_, activity) in _activites)
        // {
        //     if (activity is IDisposable disposable)
        //     {
        //         disposable.Dispose();
        //     }
        // }
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