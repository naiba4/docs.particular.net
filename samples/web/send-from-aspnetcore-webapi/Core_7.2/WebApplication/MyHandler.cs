using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class MyHandler :
    IHandleMessages<MyMessage>
{
    static ILog log = LogManager.GetLogger<MyHandler>();

    ReceiverDataContext dataContext;

    public MyHandler(ReceiverDataContext dataContext)
    {
        this.dataContext = dataContext;
    }

    #region MessageHandler

    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        log.Info("Message received at endpoint");
        return Task.CompletedTask;
    }
    #endregion
}