namespace TaskTronic.Statistics.Services.Statistics
{
    using AutoMapper;
    using Data;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Models.Statistics;
    using System.Threading.Tasks;
    using TaskTronic.Services;

    public class StatisticsService : DataService<Statistics>, IStatisticsService
    {
        private readonly IMapper mapper;

        public StatisticsService(StatisticsDbContext dbContext, IMapper mapper)
            : base(dbContext) 
            => this.mapper = mapper;

        public async Task AddFileAsync()
        {
            var statistics = await base.All().SingleOrDefaultAsync();

            statistics.TotalFiles++;

            await base.Save(statistics);
        }

        public async Task AddFolderAsync()
        {
            var statistics = await base.All().SingleOrDefaultAsync();

            statistics.TotalFolders++;

            await base.Save(statistics);
        }

        public async Task<StatisticsOutputModel> FullStatsAsync()
            => await this.mapper
                .ProjectTo<StatisticsOutputModel>(base.All())
                .FirstOrDefaultAsync();

        public async Task RemoveFileAsync()
        {
            var statistics = await base.All().SingleOrDefaultAsync();

            statistics.TotalFiles--;

            await base.Save(statistics);
        }

        public async Task RemoveFolderAsync()
        {
            var statistics = await base.All().SingleOrDefaultAsync();

            statistics.TotalFolders--;

            await base.Save(statistics);
        }
    }
}
