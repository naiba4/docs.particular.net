using NServiceBus;
using System.Threading.Tasks;
using NServiceBus.Logging;

public class RequestDataMessageHandler : IHandleMessages<RequestDataMessage>
{
    static ILog log = LogManager.GetLogger<RequestDataMessageHandler>();

    public Task Handle(RequestDataMessage message, IMessageHandlerContext context)
    {
        log.Info($"Received request {message.DataId}.");
        log.Info($"String length received: {message.String.Length}.");

        return Task.CompletedTask;
    }
}