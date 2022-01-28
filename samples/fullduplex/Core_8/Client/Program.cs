using Client;

using NServiceBus;
using NServiceBus.Logging;

using Shared;

using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        bool useAzureStorage = false;
        int messagesToSend = 1000;

        Console.Title = "BodyStorage.Test.Client";

        LogManager.Use<DefaultFactory>()
            .Level(LogLevel.Info);
        var endpointConfiguration = new EndpointConfiguration("BodyStorage.Test.Client");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseTransport(new RabbitMQTransport(Topology.Conventional, Environment.GetEnvironmentVariable("RabbitMQTransport_ConnectionString")));

        endpointConfiguration.AuditProcessedMessagesTo("audit");

        if (useAzureStorage)
        {
            var containerClient = await AzureAuditBodyStorageConfiguration.GetContainerClient();
            endpointConfiguration.Pipeline.Register(_ => new StoreAuditBodyInAzureBlobStorageBehavior(containerClient), "Writing the body to azure blobstorage");
        }

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        var payloadGenerator = new RandomPayloadGenerator(new Random(42));
        await payloadGenerator.Initialize();

        Console.CursorVisible = false;
        Console.Write("Messages sent: ");
        var cursor = Console.GetCursorPosition();

        for (int i = 0; i < messagesToSend; ++i)
        {
            Console.SetCursorPosition(cursor.Left, cursor.Top);

            var message = new RequestDataMessage
            {
                DataId = Guid.NewGuid(),
                String = payloadGenerator.GetTextBlock()
            };

            await endpointInstance.Send("BodyStorage.Test.Server", message)
                .ConfigureAwait(false);

            Console.Write(i + 1);
        }
        Console.WriteLine();

        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}