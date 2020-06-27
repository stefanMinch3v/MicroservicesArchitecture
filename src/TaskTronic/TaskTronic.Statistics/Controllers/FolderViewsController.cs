namespace TaskTronic.Statistics.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models.FolderViews;
    using Services.FolderViews;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Services.Identity;

    [Authorize]
    public class FolderViewsController : ApiController
    {
        private readonly IFolderViewService folderViewService;
        private readonly ICurrentUserService currentUser;

        public FolderViewsController(
            IFolderViewService folderViewService,
            ICurrentUserService currentUser)
        {
            this.folderViewService = folderViewService;
            this.currentUser = currentUser;
        }

        [HttpGet]
        [Route(Id)]
        public async Task<int> TotalViews(int id)
            => await this.folderViewService.GetTotalViews(id);

        [HttpPost]
        [Route(nameof(AddView))]
        public async Task AddView(int folderId)
            => await this.folderViewService.AddViewAsync(folderId, this.currentUser.UserId);

        [HttpGet]
        public async Task<IReadOnlyCollection<FolderViewOutputModel>> TotalViews([FromQuery] IEnumerable<int> ids)
            => await this.folderViewService.GetTotalViews(ids);
    }
}
