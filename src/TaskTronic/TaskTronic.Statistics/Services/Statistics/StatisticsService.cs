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

        public async Task<StatisticsOutputModel> FullStatsAsync()
            => await this.mapper
                .ProjectTo<StatisticsOutputModel>(base.All())
                .FirstOrDefaultAsync();
    }
}
