namespace TaskTronic.Statistics.Services.FolderViews
{
    using Data;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Models.FolderViews;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TaskTronic.Services;

    public class FolderViewService : DataService<FolderView>, IFolderViewService
    {
        public FolderViewService(StatisticsDbContext dbContext)
            : base(dbContext)
        { }

        public async Task<int> GetTotalViews(int folderId)
            => await base.All()
                .CountAsync(v => v.FolderId == folderId);

        public async Task<IReadOnlyCollection<FolderViewOutputModel>> GetTotalViews(IEnumerable<int> ids)
            => await base.All()
                .Where(v => ids.Contains(v.FolderId))
                .GroupBy(v => v.FolderId)
                .Select(gr => new FolderViewOutputModel
                {
                    FolderId = gr.Key,
                    TotalViews = gr.Count()
                })
                .ToListAsync();
    }
}
