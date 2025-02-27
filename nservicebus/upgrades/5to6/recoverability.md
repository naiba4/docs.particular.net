---
title: Recoverability Changes in Version 6
summary: Describes changes to the recoverability feature in NServiceBus version 6
reviewed: 2022-09-30
component: Core
related:
 - nservicebus/upgrades/5to6
 - nservicebus/recoverability
redirects:
 - nservicebus/upgrades/5to6-recoverability
isUpgradeGuide: true
upgradeGuideCoreVersions:
 - 5
 - 6
---


## New names for first and second level retries

First Level Retries (FLR) have been renamed to [Immediate Retries](/nservicebus/recoverability/#immediate-retries).

Second Level Retries (SLR) have been renamed to [Delayed Retries](/nservicebus/recoverability/#delayed-retries).


## Code-first API

Recoverability can now be configured using a code-first API:

```csharp
var recoverability = endpointConfiguration.Recoverability();
recoverability.Immediate(
    customizations: immediate =>
    {
        immediate.NumberOfRetries(3);
    });
recoverability.Delayed(
    customizations: delayed =>
    {
        var numberOfRetries = delayed.NumberOfRetries(5);
        numberOfRetries.TimeIncrease(TimeSpan.FromSeconds(30));
    });
```

The API enables setting all automatic retry parameters that were previously available only with the configuration source approach.


## Disabling immediate retries

The `TransportConfig` API is obsolete and no longer used to disable immediate retries. Equivalent behavior can be achieved through the code-first API by configuring immediate retries:

```csharp
// For NServiceBus version 6.x
var recoverability = endpointConfiguration.Recoverability();
recoverability.Immediate(
    customizations: immediate =>
    {
        immediate.NumberOfRetries(0);
    });

// For NServiceBus version 5.x
class ConfigTransport :
    IProvideConfiguration<TransportConfig>
{
    public TransportConfig GetConfiguration()
    {
        return new TransportConfig
        {
            MaxRetries = 0
        };
    }
}
```


## Disabling delayed retries

The `SecondLevelRetries` feature is not used to disable delayed retries. Equivalent behavior can be achieved through the code-first API by configuring delayed retries:

```csharp
// For NServiceBus version 6.x
var recoverability = endpointConfiguration.Recoverability();
recoverability.Delayed(
    customizations: delayed =>
    {
        delayed.NumberOfRetries(0);
    });

// For NServiceBus version 5.x
busConfiguration.DisableFeature<SecondLevelRetries>();
```


## MaxRetries value for first level retries

In NServiceBus version 5, the `MaxRetries` parameter for first level retries defined the number of immediate message deliveries, including initial delivery. As a result, setting this value to `1` resulted in no immediate retries being performed. In NServiceBus version 6, the meaning of this parameter has changed and now defines the number of immediate retries alone (excluding initial delivery).

In order to get the same behavior in NServiceBus version 6 as in the previous version, the value configured in version 6 should be one less than in version 5.


## [Custom retry policy](/nservicebus/recoverability/custom-recoverability-policy.md)

In NServiceBus version 5, custom retry policies provided the ability to control second level retries. In NServiceBus version 6, the custom retry policy concept has been substituted by a recoverability policy which enables control over every stage of automatic retries as well as error handling.

The recoverability policy operates on a `RecoverabilityConfig` and `ErrorContext` instead of a `TransportMessage` which was the case for the custom retry policy.

A custom recoverability policy can be registered using the `CustomPolicy` method:

```csharp
// For NServiceBus version 6.x
var recoverability = endpointConfiguration.Recoverability();
recoverability.Delayed(
    customizations: delayed =>
    {
        // desired number of retries
        delayed.NumberOfRetries(3);
    });
recoverability.CustomPolicy(MyCustomRetryPolicy);

// For NServiceBus version 5.x
var retriesSettings = busConfiguration.SecondLevelRetries();
retriesSettings.CustomRetryPolicy(MyCustomRetryPolicy);
```

The custom recoverability policy doesn't have to fully override the default behavior; for example, it can provide custom logic for a specific type of error. In any case, the default behavior can be reused by calling the `Invoke` method on the `DefaultRecoverabilityPolicy` class.

The following snippet is an example of a custom recoverability policy that changes the delay value calculated by the default policy.

```csharp
// For NServiceBus version 6.x
static RecoverabilityAction MyCustomRetryPolicy(RecoverabilityConfig config, ErrorContext context)
{
    var numberOfRetries = context.DelayedDeliveriesPerformed;
    var exceptionInstance = context.Exception;

    // call the default recoverability of default behavior is desired
    var action = DefaultRecoverabilityPolicy.Invoke(config, context);

    if (action is DelayedRetry delayedRetryAction)
    {
        // perform some logic and decide when to do delayed retries
        return RecoverabilityAction.DelayedRetry(TimeSpan.FromSeconds(5));
    }

    return action;
}

// For NServiceBus version 5.x
static TimeSpan MyCustomRetryPolicy(TransportMessage transportMessage)
{
    var numberOfRetries = transportMessage.NumberOfRetries();
    var exceptionType = transportMessage.ExceptionType();

    // perform some logic and decide when to retry
    return TimeSpan.FromSeconds(5);
}

static int NumberOfRetries(this TransportMessage transportMessage)
{
    var headers = transportMessage.Headers;
    if (headers.TryGetValue(Headers.Retries, out var value))
    {
        return int.Parse(value);
    }
    return 0;
}

static string ExceptionType(this TransportMessage transportMessage)
{
    var headers = transportMessage.Headers;
    return headers["NServiceBus.ExceptionInfo.ExceptionType"];
}
```

The configuration passed to the custom recoverability policy contains values configured via the recoverability configuration API.


## TransportConfig

`NServiceBus.Config.TransportConfig` has been deprecated. The same functionality can be achieved using the code API.

```csharp
var recoverability = endpointConfiguration.Recoverability();
recoverability.Immediate(
    customizations: immediate =>
    {
        immediate.NumberOfRetries(2);
    });
```

```xml
<configuration>
  <configSections>
    <section name="TransportConfig"
             type="NServiceBus.Config.TransportConfig, NServiceBus.Core"/>
  </configSections>
  <TransportConfig MaxRetries="2" />
</configuration>
```


## SecondLevelRetriesConfig

`NServiceBus.Config.SecondLevelRetriesConfig` has been deprecated. The same functionality can be achieved using the code API.

```csharp
var recoverability = endpointConfiguration.Recoverability();
recoverability.Delayed(
    customizations: delayed =>
    {
        delayed.NumberOfRetries(3);
        delayed.TimeIncrease(TimeSpan.FromSeconds(10));
    });
```

```xml
<configSections>
  <section name="SecondLevelRetriesConfig"
           type="NServiceBus.Config.SecondLevelRetriesConfig, NServiceBus.Core"/>
  </configSections>
<SecondLevelRetriesConfig Enabled="true"
                          TimeIncrease="00:00:10"
                          NumberOfRetries="3" />
```


## Legacy .Retries queue

The `.Retries` satellite queue is no longer necessary when running NServiceBus version 6. However, when starting a version 6 endpoint, unless explicitly configured, a dedicated receiver that watches the .retries queue will still be started. This default configuration is mainly for a one-time scenario necessary to prevent message loss when upgrading from version 5 to version 6.


### Upgrading from versions 5 and below

In NServiceBus versions 5 and below, the [delayed retries](/nservicebus/recoverability/#delayed-retries) of NServiceBus use the `[endpoint_name].Retries` queue to durably store messages before persisting them for retries.  When upgrading to version 6, during the process of stopping the endpoint, there is a possibility that the .retries queue may contain some of these messages that were meant to be delayed and retried. Therefore, to prevent message loss, when starting a version 6 endpoint, the `.Retries` satellite receiver executes to serve a one-time purpose of forwarding those messages from the `.Retries` queue to the endpoint's main queue to be retried appropriately.

Once the upgrade is done, the receiver can be safely disabled and the `.Retries` queue can be safely deleted.


### Creating new endpoints using version 6

In NServiceBus version 6, the only reason that the `.Retries` queue exists is so that version 5 endpoints can be migrated to version 6 without message loss. Endpoints written using NServiceBus version 6 can safely use the configuration API to disable the satellite.


## IManageMessageFailures is now obsolete.

The `IManageMessageFailures` interface was the extension point to customize the handling of second level retries before a message failure is forwarded to the error queue.

This same functionality and more can be achieved using the [message processing pipeline](/nservicebus/pipeline/). See also: [Customizing error handling with the pipeline](/nservicebus/pipeline/customizing-error-handling.md).


## RepeatedFailuresOverTimeCircuitBreaker has been made internal

Instead of using `RepeatedFailuresOverTimeCircuitBreaker`, include [the source code](https://github.com/Particular/NServiceBus/blob/5.2.5/src/NServiceBus.Core/CircuitBreakers/RepeatedFailuresOverTimeCircuitBreaker.cs) in the project.


## [Critical error action](/nservicebus/hosting/critical-errors.md)

The API for defining a [critical error action](/nservicebus/hosting/critical-errors.md) has been changed to a custom delegate.

```csharp
// For NServiceBus version 6.x
endpointConfiguration.DefineCriticalErrorAction(
    new Func<ICriticalErrorContext, Task>(context =>
    {
        // place custom handling here
        return Task.CompletedTask;
    }));

// For NServiceBus version 5.x
busConfiguration.DefineCriticalErrorAction(
    new Action<string, Exception>((error, exception) =>
    {
        // place custom handling here
    }));
```


## Notifications

The `BusNotifications` class has been renamed to `Notifications`.

`BusNotifications` previously exposed the available notification hooks as observables implementing `IObservable`. This required implementing the `IObserver` interface or including [Reactive-Extensions](https://msdn.microsoft.com/library/hh242985.aspx) to use this API. In NServiceBus version 6, the notifications API has been improved. It exposes regular events instead of observables. To continue using Reactive-Extensions, the events API can be transformed into `IObservable`s as follows:

```csharp
var errorsNotifications = notifications.Errors;
var failedMessages = Observable.FromEventPattern<EventHandler<FailedMessage>, FailedMessage>(
    handler => errorsNotifications.MessageSentToErrorQueue += handler,
    handler => errorsNotifications.MessageSentToErrorQueue -= handler);

var subscription = failedMessages
    .Subscribe(x =>
    {
        var failedMessage = x.EventArgs;
        log.Error($"Message {failedMessage.MessageId} moved to error queue", failedMessage.Exception);
    });
```

Notification subscriptions can now also be registered at configuration time on the `EndpointConfiguration.Notifications` property. See the [error notifications documentation](/nservicebus/recoverability/subscribing-to-error-notifications.md) for more details and samples.

### Access to exception details

Exception details are now available on the passed `Notifications` parameter and not as `NServiceBus.ExceptionInfo.*` headers on the provided message.

### Delayed delivery error notifications

In NServiceBus version 6 and above, the `TimeoutManager` does not provide any error notifications. When an error occurs during processing of a deferred message by the `TimeoutManager`, the message will be retried and possibly moved to the error queue. The user will not be notified about these events.

Note that in NServiceBus version 5 and below, when a user [subscribes to error notifications](/nservicebus/recoverability/subscribing-to-error-notifications.md) they receive notification in the situation described above.


## Timeout automatic retries

Previously, configuring the number of times a message will be retried by the immediate retry mechanism also determined how many times the `TimeoutManager` attempted to retry dispatching a deferred message in case an exception was thrown. Starting in NServiceBus version 6, the `TimeoutManager` attempts the dispatch five times (this number is no longer configurable). The configuration of immediate retries for non-deferred message dispatch has not been changed.
