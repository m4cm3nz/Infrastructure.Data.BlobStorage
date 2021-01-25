using Azure.Storage.Blobs;
using Infrastructure.Data.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.BlobStorage
{
    public class BlobStorageRepository : IBlobStorage
    {
        private readonly ILogger<BlobStorageRepository> logger;
        private readonly BlobServiceClient blobServiceClient;
        private readonly string container;

        public BlobStorageRepository(
            IOptions<Settings> options,
            ILogger<BlobStorageRepository> logger)
        {

            if (options?.Value == null)
                throw new Exception("No options settings was provided");

            this.logger = logger ?? throw new Exception("No logger provider was defined.");

            blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
            container = options.Value.Container;
        }
        private BlobClient GetBlobClientFrom(string fileName)
        {
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
            return blobContainerClient.GetBlobClient(fileName);
        }

        private async Task<BlobContainerClient> CreateContainerIfNotExistsAsync(string containerName)
        {
            var blobContainerClient = blobServiceClient
                .GetBlobContainerClient(containerName);

            if (!blobContainerClient.Exists())

                blobContainerClient = await blobServiceClient.CreateBlobContainerAsync(
                   containerName);

            return blobContainerClient;
        }

        private Task UploadFromString(string content, string fileName)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(byteArray);
            return Upload(stream, fileName);
        }

        private async Task UploadFromObject<T>(T content, string fileName)
        {
            var memoryStream = new MemoryStream();

            await WriteJsonAsync(
                memoryStream,
                content,
                new UTF8Encoding());

            await Upload(memoryStream, fileName);
        }

        private async Task Upload(MemoryStream stream, string fileName)
        {
            var blobContainerClient = await CreateContainerIfNotExistsAsync(container);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(stream, true);
        }

        public async Task<Stream> Download(string fileName)
        {
            try
            {
                var blobClient = GetBlobClientFrom(fileName);
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                LogMessages.DownloadFailed(logger, fileName, ex);
                return null;
            }
        }

        public bool Exists(string fileName)
        {
            var blobClient = GetBlobClientFrom(fileName);

            return blobClient.ExistsAsync()
                .GetAwaiter()
                .GetResult().Value;
        }

        public Task Upload<T>(T content, string fileName)
        {
            try
            {
                return ((content is string) ?
                    UploadFromString(content.ToString(), fileName) :
                    UploadFromObject(content, fileName));
            }
            catch (Exception ex)
            {
                var value = (content is string) ? content.ToString() : JsonConvert.SerializeObject(content);
                LogMessages.UploadFailed(logger, typeof(T).FullName, fileName, value, ex);
                return null;
            }
        }

        private static async Task WriteJsonAsync<T>(
            Stream stream,
            T @object,
            Encoding encoding = default,
            CancellationToken cancellationToken = default,
            int bufferSize = 1024,
            bool leaveOpen = true,
            bool resetStream = true)
        {
            if (!stream.CanWrite)
                throw new NotSupportedException("Can't read from stream.");

            encoding ??= new UTF8Encoding();
            using var streamWriter = new StreamWriter(stream, encoding, bufferSize, leaveOpen);
            using var jsonTextWriter = new JsonTextWriter(streamWriter);
            new JsonSerializer().Serialize(jsonTextWriter, @object);
            await jsonTextWriter.FlushAsync(cancellationToken);

            if (resetStream && stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
