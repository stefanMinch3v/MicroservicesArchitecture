namespace TaskTronic.Drive.Services.Folders
{
    public class FolderWithAccessServiceModel
    {
        public int FolderId { get; set; }
        public int CatalogId { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public int? RootId { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public int EmployeeId { get; set; }
        public bool HasAccess { get; set; }
        public int FileCount { get; set; }
        public int FolderCount { get; set; }
    }
}
