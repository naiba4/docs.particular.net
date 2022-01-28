using NServiceBus;
using NServiceBus.Logging;
using Shared;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        bool useAzureStorage = false;

        Console.Title = "BodyStorage.Test.Server";
        LogManager.Use<DefaultFactory>()
            .Level(LogLevel.Info);

        var transport = new RabbitMQTransport(Topology.Conventional, Environment.GetEnvironmentVariable("RabbitMQTransport_ConnectionString"));
        var endpointConfiguration = new EndpointConfiguration("BodyStorage.Test.Server");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UsePersistence<LearningPersistence>();
        var routing = endpointConfiguration.UseTransport(transport);

        endpointConfiguration.AuditProcessedMessagesTo("audit");
        endpointConfiguration.SendFailedMessagesTo("error");

        if(useAzureStorage)
        {
            var containerClient = await AzureAuditBodyStorageConfiguration.GetContainerClient();
            endpointConfiguration.Pipeline.Register(_ => new StoreAuditBodyInAzureBlobStorageBehavior(containerClient), "Writing the body to azure blobstorage");
        }

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}