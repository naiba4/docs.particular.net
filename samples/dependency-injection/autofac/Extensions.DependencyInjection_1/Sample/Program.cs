using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

static class Program
{
    private const string RootLifetimeTag = "MyIsolatedRoot";

    static async Task Main()
    {
        Console.Title = "Samples.Autofac";

        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterType<MyOtherService>().AsSelf().SingleInstance();
        var rootContainer = containerBuilder.Build();

        var endpointConfiguration = new EndpointConfiguration("Samples.Autofac");
        endpointConfiguration.UseTransport<LearningTransport>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<MyService>();

        var startableEndpoint = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);

        using(var scope = rootContainer.BeginLifetimeScope(RootLifetimeTag, b =>
          {
              b.Populate(serviceCollection, RootLifetimeTag);
          }))
        {
            var endpointInstance = await startableEndpoint.Start(new AutofacServiceProvider(scope))
                .ConfigureAwait(false);

            var message1 = new MyMessage();
            await endpointInstance.SendLocal(message1)
                .ConfigureAwait(false);

            var message2 = new MyMessage();
            await endpointInstance.SendLocal(message2)
                .ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
