using NUnit.Framework;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

            var fake = new Fake
            {
                Message = "Serviço remoto respondeu com um ou mais erros de cálculo."
            };

            var memoryStream = new MemoryStream();

            await BlobStorageRepository
                .WriteJsonAsync(memoryStream, fake, jsonSerializerOptions);

            using var reader = new StreamReader(memoryStream);

            var myJson = reader.ReadToEnd();

            var newFake = JsonSerializer.Deserialize<Fake>(myJson, jsonSerializerOptions);

            Assert.That(fake.Message, Is.EqualTo(newFake.Message));
        }
    }
}