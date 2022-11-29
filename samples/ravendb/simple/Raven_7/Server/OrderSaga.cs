using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Raven.Client.Documents.Session;

#region thesaga

public class OrderSaga :
    Saga<OrderSagaData>,
    IAmStartedByMessages<StartOrder>,
    IHandleTimeouts<CompleteOrder>
{
    static ILog log = LogManager.GetLogger<OrderSaga>();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<OrderSagaData> mapper)
    {
        mapper.MapSaga(sagaData => sagaData.OrderId)
            .ToMessage<StartOrder>(message => message.OrderId);
    }

    public Task Handle(StartOrder message, IMessageHandlerContext context)
    {
        var orderDescription = $"The saga for order {message.OrderId}";
        Data.OrderDescription = orderDescription;
        log.Info($"Received StartOrder message {Data.OrderId}. Starting Saga");

        var shipOrder = new ShipOrder
        {
            OrderId = message.OrderId
        };

        log.Info("Order will complete in 5 seconds");
        var timeoutData = new CompleteOrder
        {
            OrderDescription = orderDescription
        };

        return Task.WhenAll(
            context.SendLocal(shipOrder),
            RequestTimeout(context, TimeSpan.FromSeconds(5), timeoutData)
        );
    }

    public async Task Timeout(CompleteOrder state, IMessageHandlerContext context)
    {
        var finishOrder = new FinishOrder
        {
            OrderId = Data.OrderId
        };
        MarkAsComplete();
        await context.SendLocal(finishOrder);

        log.Info($"Saga with OrderId {Data.OrderId} completed");
    }
}

public class OrderDocument
{
    public string Id { get; set; }
}

#endregion