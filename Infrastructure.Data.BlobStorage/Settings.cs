namespace Infrastructure.Data.BlobStorage
{
    public class Settings
    {
        public string ConnectionString { get; set; }
        public string  Container { get; set; }
        public bool UseDefaultAzureCredential { get; set; }
        public string UrlContainer { get; set; }
    }
}
