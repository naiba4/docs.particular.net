namespace Core5.UpgradeGuides._5to6
{
    using System;
    using System.Transactions;
    using NServiceBus;
    using NServiceBus.Unicast;
    using NServiceBus.Unicast.Transport;

    class Upgrade
    {
        void CriticalError(BusConfiguration busConfiguration)
        {
            // ReSharper disable RedundantDelegateCreation

            #region 5to6CriticalError

            busConfiguration.DefineCriticalErrorAction(
                new Action<string, Exception>((error, exception) =>
                {
                    // place custom handling here
                }));

            #endregion

            // ReSharper restore RedundantDelegateCreation
        }

        void AccessBuilder(IBus bus)
        {
            #region 5to6AccessBuilder

            var builder = ((UnicastBus) bus).Builder;

            #endregion
        }

        void DelayedDelivery(IBus bus,object message)
        {
            #region 5to6delayed-delivery
            bus.Defer(TimeSpan.FromMinutes(30), message);
            // OR
            bus.Defer(new DateTime(2016, 12, 25), message);
            #endregion
        }
    }
}