namespace PdfProcessor.API.Settings
{
    public class StorageSettings
    {
        public string UploadPdfPath { get; set; } = string.Empty;
        public int MaxFileSizeMb { get; set; } = 10;
    }
}
