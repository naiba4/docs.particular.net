﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NServiceBus;

[ApiController]
[Route("")]
public class SendMessageController : Controller
{
    IMessageSession messageSession;

    #region MessageSessionInjection
    public SendMessageController(IMessageSession messageSession)
    {
        this.messageSession = messageSession;
    }
    #endregion


    #region MessageSessionUsage
    [HttpGet]
    public async Task<string> Get([FromServices]ReceiverDataContext context)
    {
        var message = new MyMessage();
        await messageSession.SendLocal(message)
            .ConfigureAwait(false);
        return "Message sent to endpoint";
    }
    #endregion
}
