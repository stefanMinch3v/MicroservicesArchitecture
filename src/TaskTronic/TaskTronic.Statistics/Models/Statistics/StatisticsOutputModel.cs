namespace TaskTronic.Statistics.Models.Statistics
{
    using Data.Models;
    using TaskTronic.Models;

    public class StatisticsOutputModel : IMapFrom<Statistics>
    {
        public int TotalFolders { get; set; }

        public int TotalFiles { get; set; }
    }
}
