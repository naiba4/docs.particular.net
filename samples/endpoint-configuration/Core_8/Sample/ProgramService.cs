using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NServiceBus;
using NServiceBus.Extensions.Logging;
using NServiceBus.Logging;
using LogManager = NServiceBus.Logging.LogManager;

[DesignerCategory("Code")]
class ProgramService :
    ServiceBase
{
    IEndpointInstance endpointInstance;
    static readonly ILog log = LogManager.GetLogger("ProgramService");

    static void Main()
    {
        using (var service = new ProgramService())
        {
            if (ServiceHelper.IsService())
            {
                Run(service);
                return;
            }
            Console.Title = "Samples.FirstEndpoint";
            service.OnStart(null);

            Console.WriteLine("\r\nEndpoint created and configured; press any key to stop program\r\n");
            Console.ReadKey();

            service.OnStop();
        }
    }

    protected override void OnStart(string[] args)
    {
        AsyncOnStart().GetAwaiter().GetResult();
    }

    async Task AsyncOnStart()
    {
        #region logging

        var config = new LoggingConfiguration();

        var consoleTarget = new ColoredConsoleTarget
        {
            Layout = "${level}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}"
        };
        config.AddTarget("console", consoleTarget);
        config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, consoleTarget));

        NLog.LogManager.Configuration = config;

        var extensionsLoggerFactory = new NLogLoggerFactory();

        var nservicebusLoggerFactory = new ExtensionsLoggerFactory(loggerFactory: extensionsLoggerFactory);

        LogManager.UseFactory(loggerFactory:nservicebusLoggerFactory);

        #endregion

        #region create-config

        var endpointConfiguration = new EndpointConfiguration("Samples.FirstEndpoint");

        #endregion

        #region container

        endpointConfiguration.RegisterComponents(svc =>
        {
            // configure custom services
            // svc.AddSingleton(new MyService());
        });

        #endregion

        #region serialization

        endpointConfiguration.UseSerialization<XmlSerializer>();

        #endregion

        #region error

        endpointConfiguration.SendFailedMessagesTo("error");

        #endregion

        #region audit

        endpointConfiguration.AuditProcessedMessagesTo("audit");

        #endregion

        #region transport

        endpointConfiguration.UseTransport<LearningTransport>();

        #endregion

        #region persistence

        endpointConfiguration.UsePersistence<LearningPersistence>();

        #endregion

        #region critical-errors

        endpointConfiguration.DefineCriticalErrorAction(
            onCriticalError: async (context, cancellation) =>
            {
                // Log the critical error
                log.Fatal($"CRITICAL: {context.Error}", context.Exception);

                await context.Stop(cancellation)
                    .ConfigureAwait(false);

                // Kill the process on a critical error
                var output = $"NServiceBus critical error:\n{context.Error}\nShutting down.";
                Environment.FailFast(output, context.Exception);
            });

        #endregion

        #region start-bus

        endpointConfiguration.EnableInstallers();
        endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        #endregion

        var myMessage = new MyMessage();
        await endpointInstance.SendLocal(myMessage)
            .ConfigureAwait(false);
    }


    protected override void OnStop()
    {
        #region stop-endpoint

        endpointInstance?.Stop().GetAwaiter().GetResult();

        #endregion
    }
}