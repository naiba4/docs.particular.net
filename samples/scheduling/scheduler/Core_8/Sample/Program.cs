using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading.Tasks;

class Program
{
    static ILog log = LogManager.GetLogger<Program>();

    [Obsolete]
    static async Task Main()
    {
        Console.Title = "Samples.Scheduling";
        var endpointConfiguration = new EndpointConfiguration("Samples.Scheduling");
        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseTransport<LearningTransport>();

        var endpointInstance = await Endpoint.Start(endpointConfiguration);
        #region Schedule

        // Send a message every 5 seconds
        await endpointInstance.ScheduleEvery(
                timeSpan: TimeSpan.FromSeconds(5),
                task: pipelineContext =>
                {
                    var message = new MyMessage();
                    return pipelineContext.SendLocal(message);
                });

        // Name a schedule task and invoke it every 5 seconds
        await endpointInstance.ScheduleEvery(
                timeSpan: TimeSpan.FromSeconds(5),
                name: "MyCustomTask",
                task: pipelineContext =>
                {
                    log.Info("Custom Task executed");
                    return Task.CompletedTask;
                });

        #endregion

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop();
    }
}