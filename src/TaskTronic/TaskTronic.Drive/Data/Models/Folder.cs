namespace TaskTronic.Drive.Data.Models
{
    using System;

    public class Folder
    {
        public int FolderId { get; set; }
        public int CatalogId { get; set; }
        public int? ParentId { get; set; }
        public int? RootId { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public string UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}