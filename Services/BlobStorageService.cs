using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnderlordLeagueTables.Pages
{
    public class BlobStorageService
    {
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;

        public BlobStorageService(string connectionString)
        {
            var strageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = strageAccount.CreateCloudBlobClient();
        }

        public static async Task<BlobStorageService> CreateAsync(string connectionString, string containerName)
        {
            var instance = new BlobStorageService(connectionString);
            await instance.InitializeAsync(containerName);
            return instance;
        }

        public async Task InitializeAsync(string containerName)
        {
            _blobContainer = _blobClient.GetContainerReference(containerName);
            await _blobContainer.CreateIfNotExistsAsync();
            await _blobContainer.SetPermissionsAsync(
                new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        public async Task<Uri> UploadFromStreamAsync(Stream stream, string directoryName, string blobName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var blob = directory.GetBlockBlobReference(blobName);
            await blob.UploadFromStreamAsync(stream);
            return blob.Uri;
        }

        public async Task UploadFileAsync(string pathOnServer, string blobName)
        {
            using (var fileStream = File.OpenRead(pathOnServer))
            {
                var filename = Path.GetFileName(blobName); // Trim fully pathed filename to just the filename
                var blockBlob = _blobContainer.GetBlockBlobReference(filename);
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }
        public async Task DeleteAsync(string directoryName, string blobName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var blob = directory.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }


        public async Task DownloadAsync(string blobName, string pathOnServer)
        {
            var blob = _blobContainer.GetBlockBlobReference(blobName);
            var ms = new MemoryStream();
            using (var fileStream = File.OpenWrite(pathOnServer))
            {
                await blob.DownloadToStreamAsync(fileStream);
            }
        }

        public async Task DeleteDirectoryAsync(string directoryName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            foreach (CloudBlob blob in (await directory.ListBlobsSegmentedAsync(new BlobContinuationToken())).Results)
            {
                await blob.DeleteIfExistsAsync();
            }
        }

        public async Task<IEnumerable<Uri>> ListBlobUri(string directoryName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            return (await directory.ListBlobsSegmentedAsync(new BlobContinuationToken())).Results.Select(blob => blob.Uri);
        }
    }
}