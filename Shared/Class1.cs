using System.ComponentModel;

namespace Shared;

public interface IActivity : IDisposable
{
    void Execute(Context context);
}
public abstract class Activity : IActivity
{
    public virtual void Dispose()
    {
    }

    public abstract void Execute(Context context);
}
public class Context
{
    public Dictionary<string, object?> Stuff { get; set; } = new();
}
public interface IExtensionSetup
{
    IServiceProvider GetProvider();
}