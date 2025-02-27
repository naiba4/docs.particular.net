﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using Azure.Messaging.ServiceBus.Administration;
using LockRenewal;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.ASB.LockRenewal";

        var endpointConfiguration = new EndpointConfiguration("Samples.ASB.SendReply.LockRenewal");
        endpointConfiguration.EnableInstallers();

        var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Could not read the 'AzureServiceBus_ConnectionString' environment variable. Check the sample prerequisites.");
        }

        var transport = new AzureServiceBusTransport(connectionString);
        transport.PrefetchCount = 0;

        endpointConfiguration.UseTransport(transport);

        var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

        await OverrideQueueLockDuration("Samples.ASB.SendReply.LockRenewal", connectionString, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

        await endpointInstance.SendLocal(new LongProcessingMessage { ProcessingDuration = TimeSpan.FromSeconds(45) });

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        await endpointInstance.Stop().ConfigureAwait(false);
    }

    private static async Task OverrideQueueLockDuration(string queuePath, string connectionString, TimeSpan lockDuration)
    {
        var managementClient = new ServiceBusAdministrationClient(connectionString);

        var queueDescription = await managementClient.GetQueueAsync(queuePath).ConfigureAwait(false);
        queueDescription.Value.LockDuration = lockDuration;

        await managementClient.UpdateQueueAsync(queueDescription.Value).ConfigureAwait(false);
    }
}