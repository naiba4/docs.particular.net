using System;
using Messages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace ClientUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = EndpointName;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                        .UseNServiceBus(context =>
                        {
                            var endpointConfiguration = new EndpointConfiguration(EndpointName);

                            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                            transport.ConnectionString("enter-connectionstring");

                            endpointConfiguration.SendFailedMessagesTo("error");
                            endpointConfiguration.AuditProcessedMessagesTo("audit");
                            endpointConfiguration.SendHeartbeatTo("Particular.ServiceControl");

                            endpointConfiguration.EnableInstallers();

                            var metrics = endpointConfiguration.EnableMetrics();
                            metrics.SendMetricDataToServiceControl("Particular.Monitoring", TimeSpan.FromMilliseconds(500));

                            return endpointConfiguration;

                        })
                       .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                        });
        }

        public static string EndpointName => "ClientUI";
    }
}
