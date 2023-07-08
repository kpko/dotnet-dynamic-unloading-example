using Shared;

using UnloadingTests;


static void Test(out WeakReference instanceRef)
{
    var path = Path.GetFullPath("../../../../Lib/bin/Release/net6.0/publish/Lib.dll");
    var instance = AssemblyExtensionInstance.LoadFromAssembly(path);

    Console.WriteLine(instance.IsLoaded);

    var ctx = new Context();

    instance.CallTestMethod("Test", ctx);
    instance.CallTestMethod("Test2", ctx);

    Console.WriteLine(instance.IsLoaded);

    // Try to force cleanup to make sure nothing is cleaned up to early
    for (var i = 0; instance.IsLoaded && (i < 10); i++)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    instance.CallTestMethod("Test2", ctx);

    // Unload the extension instance, this also unloads the service provider of the extension
    instance.Unload();

    foreach (var (key, value) in ctx.Stuff)
    {
        Console.WriteLine($"{key}: {value}");
    }
    
    instanceRef = instance.Ref;
}

Test(out var instance);

// Try clean up the ref for at least 10 times
for (var i = 0; instance.IsAlive && (i < 10); i++)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
}

Console.WriteLine(instance.IsAlive);