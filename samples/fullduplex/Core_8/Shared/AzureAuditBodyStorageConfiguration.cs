namespace Shared
{
    using Azure.Storage.Blobs;
    using System;
    using System.Threading.Tasks;

    public static class AzureAuditBodyStorageConfiguration
    {
        public static async Task<BlobContainerClient> GetContainerClient()
        {
            var blobClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureStorage_ConnectionString"));

            var blobContainerClient = blobClient.GetBlobContainerClient("audit-bodies");
            await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            return blobContainerClient;
        }
    }
}