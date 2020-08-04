namespace TaskTronic.Drive.Models.Folders
{
    using System.Collections.Generic;

    public class FolderSearchServiceModel
    {
        public int FolderId { get; set; }
        public int CatalogId { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public int? RootId { get; set; }
        public bool IsPrivate { get; set; }

        public IList<FolderSearchServiceModel> SubFolders { get; set; } = new List<FolderSearchServiceModel>();

        public override string ToString()
        {
            return $"{this.FolderId} - {this.Name}";
        }
    }
}
