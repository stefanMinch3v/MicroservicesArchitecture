namespace TaskTronic.Drive.Data.Models
{
    using Common;

    public class File : DateTrack
    {
        public int FileId { get; set; }
        public int CatalogId { get; set; }
        public int FolderId { get; set; }
        public int BlobId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long Filesize { get; set; }
        public string ContentType { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
