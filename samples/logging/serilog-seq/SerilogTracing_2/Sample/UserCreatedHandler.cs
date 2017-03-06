﻿using NServiceBus;
using NServiceBus.Logging;

public class UserCreatedHandler :
    IHandleMessages<UserCreated>
{
    static ILog log = LogManager.GetLogger(typeof(UserCreatedHandler));

    public void Handle(UserCreated message)
    {
        log.InfoFormat("Hello from {@Handler}", nameof(UserCreatedHandler));
    }
}