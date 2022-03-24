using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

[ApiController]
[Route("")]
public class SendMessageController : Controller
{
    IMessageSession messageSession;
    readonly ReceiverDataContext dataContext;

    #region MessageSessionInjection
    public SendMessageController(IMessageSession messageSession, ReceiverDataContext dataContext)
    {
        this.messageSession = messageSession;
        this.dataContext = dataContext;
    }
    #endregion


    #region MessageSessionUsage
    [HttpGet]
    public async Task<string> Get()
    {
        var message = new MyMessage();
        await messageSession.SendLocal(message)
            .ConfigureAwait(false);
        return "Message sent to endpoint";
    }
    #endregion
}
