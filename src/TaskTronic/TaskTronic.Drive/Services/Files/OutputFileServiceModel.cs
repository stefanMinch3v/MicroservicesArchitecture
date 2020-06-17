namespace TaskTronic.Drive.Services.Files
{
    public class OutputFileServiceModel
    {
        public int FileId { get; set; }
        public int CatalogId { get; set; }
        public int FolderId { get; set; }
        public int BlobId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string UpdaterUsername { get; set; }
        public string UpdaterId { get; set; }
    }
}
