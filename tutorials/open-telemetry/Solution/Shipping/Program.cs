﻿using Messages;
using NServiceBus;
using System;

namespace Shipping
{
    using Azure.Monitor.OpenTelemetry.Exporter;
    using Honeycomb.OpenTelemetry;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using System.Diagnostics;

    class Program
    {
        static void Main(string[] args)
        {
            var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                ActivityStopped = activity =>
                {
                    foreach (var (key, value) in activity.Baggage)
                    {
                        activity.AddTag(key, value);
                    }
                }
            };
            ActivitySource.AddActivityListener(listener);

            CreateHostBuilder(args).Build().Run();
            Console.Title = EndpointName;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseNServiceBus(hostBuilderContext =>
                {
                    var endpointConfiguration = new EndpointConfiguration("Shipping");

                    var transport = endpointConfiguration.UseTransport<LearningTransport>();
                    var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();

                    var routing = transport.Routing();
                    routing.RouteToEndpoint(typeof(ShipOrder), "Shipping");
                    routing.RouteToEndpoint(typeof(ShipWithMaple), "Shipping");
                    routing.RouteToEndpoint(typeof(ShipWithAlpine), "Shipping");

                    return endpointConfiguration;
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddOpenTelemetryTracing(builder => builder
                                                                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(EndpointName))
                                                                .AddSource("NServiceBus")
                                                                .AddJaegerExporter(c =>
                                                                {
                                                                    c.AgentHost = "localhost";
                                                                    c.AgentPort = 6831;
                                                                })
                                                                .AddAzureMonitorTraceExporter(c => { c.ConnectionString = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY"); })
                                                                .AddHoneycomb(new HoneycombOptions
                                                                {
                                                                    ServiceName = "spike",
                                                                    ApiKey = Environment.GetEnvironmentVariable("HONEYCOMB_APIKEY"),
                                                                    Dataset = "spike-core"
                                                                })
                    );
                });


        public static string EndpointName => "Shipping";
    }
}