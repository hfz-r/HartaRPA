namespace Harta.Services.File.API
{
    public class FileSettings
    {
        public string ConnectionString { get; set; }
        public SourceFile SourceFile { get; set; }
    }

    public class SourceFile
    {
        public string Folder { get; set; }
        public string[] Headers { get; set; }
    }
}