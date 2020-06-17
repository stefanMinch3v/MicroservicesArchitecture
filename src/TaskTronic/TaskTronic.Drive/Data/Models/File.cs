namespace TaskTronic.Drive.Data.Models
{
    using System;

    public class File
    {
        public int FileId { get; set; }
        public int CatalogId { get; set; }
        public int FolderId { get; set; }
        public int BlobId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long Filesize { get; set; }
        public string ContentType { get; set; }
        public string UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
