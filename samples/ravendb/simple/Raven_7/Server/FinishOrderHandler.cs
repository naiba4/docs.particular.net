using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Raven.Client.Documents;

public class FinishOrderHandler : IHandleMessages<FinishOrder>
{
    private readonly IDocumentStore documentStore;
    static ILog log = LogManager.GetLogger<FinishOrderHandler>();

    public FinishOrderHandler(IDocumentStore documentStore)
    {
        this.documentStore = documentStore;
    }
    public async Task Handle(FinishOrder message, IMessageHandlerContext context)
    {
        using (var session = documentStore.OpenSession())
        {
            log.Info($"Finish handler with OrderId {message.OrderId} started");
            var orderCompleted = new OrderCompleted
            {
                OrderId = message.OrderId
            };
            session.Store(new OrderDocument { Id = message.OrderId.ToString() });
            await context.Publish(orderCompleted);

            log.Info($"Order completed {orderCompleted.OrderId} published");

            session.SaveChanges();

            log.Info($"Changes for {orderCompleted.OrderId} saved");
        }
    }
}