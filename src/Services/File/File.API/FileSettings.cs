namespace Harta.Services.File.API
{
    public class FileSettings
    {
        public string SourceFolder { get; set; }
        public string FileExtension { get; set; }
    }

    public class ConnectionStrings
    {
        public string IntegrationEventConnStr { get; set; }
        public string MappingSvcConnStr { get; set; }
    }
}