namespace TaskTronic.Statistics.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models.FolderViews;
    using Services.FolderViews;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;

    public class FolderViewsController : ApiController
    {
        private readonly IFolderViewService folderViewService;

        public FolderViewsController(IFolderViewService folderViewService)
            => this.folderViewService = folderViewService;

        [HttpGet]
        [Route(Id)]
        public async Task<int> TotalViews(int id)
            => await this.folderViewService.GetTotalViews(id);

        [HttpGet]
        [Authorize]
        public async Task<IReadOnlyCollection<FolderViewOutputModel>> TotalViews([FromQuery] IEnumerable<int> ids)
            => await this.folderViewService.GetTotalViews(ids);
    }
}
