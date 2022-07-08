using NServiceBus;

class Transactions
{
    void DistributedTransactions(EndpointConfiguration endpointConfiguration)
    {
        #region 2to3-enable-ambient-transaction

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.Transactions(TransportTransactionMode.TransactionScope);

        #endregion
    }
}