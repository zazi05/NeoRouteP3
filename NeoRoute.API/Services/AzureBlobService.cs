using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NeoRoute.API.Services
{
    public class AzureBlobService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobService> _logger;

        public AzureBlobService(IConfiguration configuration, ILogger<AzureBlobService> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "event-images";

            if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("PUT_YOUR"))
            {
                // Allow app to start without real Azure for local/dev; upload will fail gracefully
                _logger.LogWarning("Azure Blob Storage connection string is not configured. File uploads will not work.");
                _containerClient = null!;
                return;
            }

            _containerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task EnsureContainerAsync()
        {
            if (_containerClient == null) return;
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        public async Task<string?> UploadAsync(Stream content, string fileName, string contentType)
        {
            if (_containerClient == null)
                throw new InvalidOperationException("Azure Blob Storage is not configured.");

            await EnsureContainerAsync();

            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var headers = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteAsync(string blobUrl)
        {
            if (_containerClient == null || string.IsNullOrEmpty(blobUrl)) return;

            try
            {
                var uri = new Uri(blobUrl);
                var blobName = uri.Segments.Last();
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete blob {Url}", blobUrl);
            }
        }
    }
}
