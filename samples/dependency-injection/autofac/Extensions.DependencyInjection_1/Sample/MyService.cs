using System.Threading;
using Autofac;
using NServiceBus.Logging;

public class MyService
{
    static ILog log = LogManager.GetLogger<MyService>();

    static long instanceNumber = 0;
    readonly string tag;

    public MyService(ILifetimeScope lifetimeScope)
    {
        Interlocked.Increment(ref instanceNumber);
        tag = lifetimeScope.Tag as string;
    }

    public void WriteHello()
    {
        log.Info($"Hello from MyService{Interlocked.Read(ref instanceNumber)} managed on container '{tag}'.");
    }
}

public class MyOtherService
{
    static ILog log = LogManager.GetLogger<MyOtherService>();

    static long instanceNumber = 0;
    readonly string tag;

    public MyOtherService(ILifetimeScope lifetimeScope)
    {
        Interlocked.Increment(ref instanceNumber);
        tag = lifetimeScope.Tag as string;
    }

    public void WriteHello()
    {
        log.Info($"Hello from MyOtherService{Interlocked.Read(ref instanceNumber)} managed on container '{tag}'.");
    }
}