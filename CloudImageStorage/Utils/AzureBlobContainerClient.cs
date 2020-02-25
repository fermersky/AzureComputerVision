using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudImageStorage.Utils
{
    public class AzureBlobContainerClient
    {
        private readonly BlobServiceClient serviceClient;
        private readonly string containerName;
        private BlobContainerClient blobContainer { get => serviceClient.GetBlobContainerClient(containerName); }

        public AzureBlobContainerClient(string connectionString, string containerName)
        {
            this.serviceClient = new BlobServiceClient(connectionString);
            this.containerName = containerName;
        }

        public async Task<BlobClient> UploadFileAsync(string fileName, Stream stream)
        {
            await blobContainer.UploadBlobAsync(fileName, stream);
            var blob = blobContainer.GetBlobClient(fileName);

            return blob;
        }

        public BlobClient GetBlobByName(string blobName)
        {
            return blobContainer.GetBlobClient(blobName);
        }

        public async Task DeleteFileAsync(string blobName)
        {
            await blobContainer.GetBlobClient(blobName).DeleteIfExistsAsync();
        }
    }
}
