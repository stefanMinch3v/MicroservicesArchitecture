﻿namespace TaskTronic.Drive.Data.Models
{
    using Common;

    public class Folder : DateTrack
    {
        public int FolderId { get; set; }
        public int CatalogId { get; set; }
        public int? ParentId { get; set; }
        public int? RootId { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}