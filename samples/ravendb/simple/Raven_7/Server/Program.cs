using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.Expiration;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.RavenDB.Server";

        #region Config

        var endpointConfiguration = new EndpointConfiguration("Samples.RavenDB.Server");
        endpointConfiguration.SendFailedMessagesTo("error");
        using (var documentStore = new DocumentStore
        {
            Urls = new[] { "http://localhost:8083", "http://localhost:8081", "http://localhost:8082" },
            Database = "RavenDbSample",
        })
        {
            documentStore.Initialize();

            var persistence = endpointConfiguration.UsePersistence<RavenDBPersistence>();
            persistence.SetDefaultDocumentStore(documentStore);
            persistence.EnableClusterWideTransactions();

            endpointConfiguration.RegisterComponents(c => c.ConfigureComponent<IDocumentStore>(() => documentStore, DependencyLifecycle.SingleInstance));

            #endregion

            var outbox = endpointConfiguration.EnableOutbox();
            outbox.SetTimeToKeepDeduplicationData(TimeSpan.FromMinutes(5));

            var transport = endpointConfiguration.UseTransport<MsmqTransport>();
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            endpointConfiguration.EnableInstallers();

            await EnsureDatabaseExistsAndExpirationEnabled(documentStore);

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }

    static async Task EnsureDatabaseExistsAndExpirationEnabled(DocumentStore documentStore)
    {
        // create the database
        try
        {
            await documentStore.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(documentStore.Database)
            {
                Topology = new DatabaseTopology
                {
                    ReplicationFactor = 3,
                    Members = new List<string>
                    {
                        "A",
                        "B",
                        "C"
                    }
                }
            }));
        }
        catch (ConcurrencyException)
        {
            // intentionally ignored
        }

        // enable document expiration
        await documentStore.Maintenance.SendAsync(new ConfigureExpirationOperation(new ExpirationConfiguration { Disabled = false, }));
    }
}