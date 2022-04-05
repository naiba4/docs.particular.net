using System.Threading.Tasks;
using NServiceBus;
#region InjectingDependency
public class MyHandler :
    IHandleMessages<MyMessage>
{
    readonly MyService myService;
    readonly MyOtherService myOtherService;

    public MyHandler(MyService myService, MyOtherService myOtherService)
    {
        this.myOtherService = myOtherService;
        this.myService = myService;
    }

    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        myService.WriteHello();
        myOtherService.WriteHello();
        return Task.CompletedTask;
    }
}
#endregion
