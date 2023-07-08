using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shared;

namespace Lib;

public class Setup : IExtensionSetup
{
    public IServiceProvider GetProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(b => b.AddConsole());
        serviceCollection.AddSingleton<IActivity, Test>();
        serviceCollection.AddSingleton<IActivity, Test2>();
        return serviceCollection.BuildServiceProvider();
    }
}
public class Test : Activity
{
    private readonly ILogger<Test> _logger;

    public Test(ILogger<Test> logger)
    {
        _logger = logger;
    }

    public override void Execute(Context context)
    {
        SqlConnection.ClearAllPools();
        _logger.LogInformation("Execute from activity 1");
    }
}
public class Test2 : Activity
{
    public override void Execute(Context context)
    {
        Console.WriteLine("Execute from activity 2");
        context.Stuff.Add(DateTime.Now.Ticks.ToString(), "asldkjasldkjalkjd");
    }
}