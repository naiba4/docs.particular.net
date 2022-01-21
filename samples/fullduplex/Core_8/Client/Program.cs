using NServiceBus;
using NServiceBus.Logging;
using Shared;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.FullDuplex.Client";
        LogManager.Use<DefaultFactory>()
            .Level(LogLevel.Info);
        var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Client");
        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseTransport(new LearningTransport());

        endpointConfiguration.AuditProcessedMessagesTo("audit");
        var containerClient = await AzureAuditBodyStorageConfiguration.GetContainerClient();

        endpointConfiguration.Pipeline.Register(_ => new StoreAuditBodyInAzureBlobStorageBehavior(containerClient), "Writing the body to azure blobstorage");


        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press enter to send a message");
        Console.WriteLine("Press any key to exit");

        #region ClientLoop

        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();

            if (key.Key != ConsoleKey.Enter)
            {
                break;
            }
            var guid = Guid.NewGuid();
            Console.WriteLine($"Requesting to get data by id: {guid:N}");

            var message = new RequestDataMessage
            {
                DataId = guid,
                String = "String property value"
            };
            await endpointInstance.Send("Samples.FullDuplex.Server", message)
                .ConfigureAwait(false);
        }

        #endregion
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}