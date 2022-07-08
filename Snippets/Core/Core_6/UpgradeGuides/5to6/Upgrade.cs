namespace Core6.UpgradeGuides._5to6
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using NServiceBus;
    using NServiceBus.ConsistencyGuarantees;
    using NServiceBus.MessageMutator;
    using NServiceBus.Settings;

    class Upgrade
    {
        #region 5to6ReAddWinIdNameHeader

        public class WinIdNameMutator :
            IMutateOutgoingTransportMessages
        {
            public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
            {
                var currentIdentity = Thread.CurrentPrincipal.Identity;
                context.OutgoingHeaders["WinIdName"] = currentIdentity.Name;
                return Task.CompletedTask;
            }
        }

        #endregion

        void StaticHeaders(EndpointConfiguration endpointConfiguration)
        {
            #region 5to6header-static-endpoint

            endpointConfiguration.AddHeaderToAllOutgoingMessages("MyGlobalHeader", "some static value");

            #endregion
        }


        void CriticalError(EndpointConfiguration endpointConfiguration)
        {
            // ReSharper disable RedundantDelegateCreation
            // ReSharper disable ConvertToLambdaExpression

            #region 5to6CriticalError

            endpointConfiguration.DefineCriticalErrorAction(
                new Func<ICriticalErrorContext, Task>(context =>
                {
                    // place custom handling here
                    return Task.CompletedTask;
                }));

            #endregion

            // ReSharper restore RedundantDelegateCreation
            // ReSharper restore ConvertToLambdaExpression
        }

        async Task DelayedDelivery(IMessageHandlerContext handlerContext, object message)
        {
            #region 5to6delayed-delivery

            var sendOptions = new SendOptions();
            sendOptions.DelayDeliveryWith(TimeSpan.FromMinutes(30));
            // OR
            sendOptions.DoNotDeliverBefore(new DateTime(2016, 12, 25));

            await handlerContext.Send(message, sendOptions)
                .ConfigureAwait(false);

            #endregion
        }
    }
}
