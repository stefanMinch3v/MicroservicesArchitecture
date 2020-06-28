namespace TaskTronic.Statistics.Services.Statistics
{
    using Models.Statistics;
    using System.Threading.Tasks;

    public interface IStatisticsService
    {
        Task<StatisticsOutputModel> FullStatsAsync();

        Task AddFolderAsync();

        Task AddFileAsync();
    }
}
