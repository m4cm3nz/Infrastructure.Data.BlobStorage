using NUnit.Framework;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Data.BlobStorage.Tests
{
    public class Fake
    {
        public string Message { get; set; }
    }

    //TODO: Write some descent tests
    public class BlobStorageRepositoryTests
    {
        [Test]
        public async Task WriteJsonAsyncTest()
        {
            var fake = new Fake
            {
                Message = "Serviço remoto respondeu com um ou mais erros de cálculo."
            };

            var memoryStream = new MemoryStream();

            await BlobStorageRepository
                .WriteJsonAsync(memoryStream, fake);

            using var reader = new StreamReader(memoryStream);

            var myJson = reader.ReadToEnd();

            var newFake = JsonSerializer.Deserialize<Fake>(myJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            Assert.AreEqual(fake.Message, newFake.Message);
        }
    }
}