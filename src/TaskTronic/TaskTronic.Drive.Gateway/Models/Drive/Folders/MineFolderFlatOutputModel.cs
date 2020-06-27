namespace TaskTronic.Drive.Gateway.Models.Drive.Folders
{
    using TaskTronic.Models;

    public sealed class MineFolderFlatOutputModel : OutputFolderFlatModel, IMapFrom<OutputFolderFlatModel>
    {
        public int TotalViews { get; set; }
    }
}
