namespace TaskTronic.Drive.Services.Folders
{
    using Services.Files;
    using System;
    using System.Collections.Generic;

    public class FolderServiceModel
    {
        public int FolderId { get; set; }
        public int CatalogId { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public int? RootId { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public int EmployeeId { get; set; }
        public int FileCount { get; set; }
        public int FolderCount { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public string UpdaterUsername { get; set; }

        public IEnumerable<FileServiceModel> Files { get; set; } = new List<FileServiceModel>();
        public IEnumerable<FolderServiceModel> SubFolders { get; set; } = new List<FolderServiceModel>();
    }
}
