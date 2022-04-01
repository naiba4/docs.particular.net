using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

static class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.Autofac";

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<MyService>();
        var endpointConfiguration = new EndpointConfiguration("Samples.Autofac");
        endpointConfiguration.UseTransport<LearningTransport>();

        var startableEndpoint = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(serviceCollection);
        var autofacContainer = containerBuilder.Build();
        var endpointInstance = await startableEndpoint.Start(new AutofacServiceProvider(autofacContainer))
            .ConfigureAwait(false);

        var myMessage = new MyMessage();
        await endpointInstance.SendLocal(myMessage)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}
