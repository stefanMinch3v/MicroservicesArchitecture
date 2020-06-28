namespace TaskTronic.Drive.Services.Folders
{
    using Data.DapperRepo;
    using Exceptions;
    using MassTransit;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using TaskTronic.Common;
    using TaskTronic.Drive.Services.Employees;
    using TaskTronic.Messages.Drive.Folders;

    public class FolderService : IFolderService
    {
        private readonly IFolderDAL folderDAL;
        private readonly IPermissionsDAL permissionsDAL;
        private readonly IFileDAL fileDAL;
        private readonly IEmployeeService employeeService;
        private readonly IBus publisher;

        public FolderService(
            IFolderDAL folderDAL,
            IPermissionsDAL permissionsDAL,
            IFileDAL file,
            IEmployeeService employeeService,
            IBus publisher)
        {
            this.folderDAL = folderDAL;
            this.permissionsDAL = permissionsDAL;
            this.fileDAL = file;
            this.employeeService = employeeService;
            this.publisher = publisher;
        }

        public async Task<bool> CreateFolderAsync(InputFolderServiceModel inputModel)
        {
            this.ValidateInput(inputModel);
            await this.ValidateParentAndRootAsync(inputModel);

            var folderId = await folderDAL.CreateFolderAsync(inputModel);

            if (folderId > 0 && inputModel.IsPrivate)
            {
                await this.permissionsDAL.CreateFolderPermissionsAsync(folderId, inputModel);
            }

            // send message to the subscribers
            await this.publisher.Publish(new FolderCreatedMessage
            {
                FolderId = folderId
            });

            return folderId > 0;
        }

        public async Task<bool> RenameFolderAsync(int catalogId, int folderId, int employeeId, string newFolderName)
        {
            Guard.AgainstEmptyString<FolderException>(newFolderName, nameof(newFolderName));
            Guard.AgainstInvalidWindowsCharacters<FolderException>(newFolderName, newFolderName);

            var folder = await this.folderDAL.GetFolderByIdAsync(folderId);

            await this.ValidateFolderAndCheckPermissionsAsync(catalogId, employeeId, folder);

            return await this.folderDAL.RenameFolderAsync(catalogId, folderId, newFolderName);
        }

        public async Task<bool> MoveFolderAsync(
            int catalogId, 
            int folderId, 
            int employeeId,
            int newParentFolderId)
        {
            var folder = await this.GetFolderByIdAsync(folderId, employeeId);

            if (folder.RootId is null)
            {
                return false;
            }

            await this.ValidateFolderAndCheckPermissionsAsync(catalogId, employeeId, folder);

            var newFolder = await this.GetFolderByIdAsync(newParentFolderId, employeeId);
            Guard.AgainstNullObject<FolderException>(newFolder, nameof(newFolder));

            if (newFolder.IsPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catalogId, newParentFolderId, employeeId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "You dont have access to this folder." };
                }
            }

            // check if moving into own subfolder
            var movingToSuccessor = await this.CheckMoveToChild(folder.RootId.Value, newParentFolderId, employeeId);

            if (!movingToSuccessor)
            {
                throw new FolderException { Message = "Cant move folder to its own successor." };
            }

            if (newFolder.SubFolders != null && newFolder.SubFolders.Any(f => f.Name == folder.Name))
            {
                var nameExists = true;
                var name = folder.Name;

                while (nameExists)
                {
                    // check if folder has " (x) "
                    var match = Regex.Match(name, @"(.+)\((\d+)\)");

                    // if not add (1)
                    if (!match.Success)
                    {
                        name += " (1)";
                    }
                    else
                    {
                        // else add (x+1)
                        int.TryParse(match.Groups[2].Value, out int currentCount);

                        name = name.Replace($"({currentCount})", $"({currentCount + 1})");
                    }

                    if (!newFolder.SubFolders.Any(f => f.Name == name))
                    {
                        nameExists = false;
                        folder.Name = name;
                    }
                }
            }

            return await this.folderDAL.MoveFolderToNewParentAsync(catalogId, folder.FolderId, newFolder.FolderId, folder.Name);
        }

        private async Task<bool> CheckMoveToChild(int folderId, int newParentId, int employeeId)
        {
            var folderTree = await this.folderDAL.GetFolderTreeAsync(folderId, employeeId);
            var rootTree = this.MapToFolderModel(folderTree.First(f => f.ParentId is null && f.RootId is null));

            var foldersToFindSubfoldersFor = new List<FolderServiceModel> { rootTree };

            while (foldersToFindSubfoldersFor.Any())
            {
                var f = foldersToFindSubfoldersFor.First();
                f.SubFolders = folderTree
                    .Where(x => x.HasAccess && x.ParentId == f.FolderId)
                    .Select(MapToFolderModel)
                    .ToList();

                foldersToFindSubfoldersFor.Remove(f);
                foldersToFindSubfoldersFor.AddRange(f.SubFolders);
            }

            var folderToCheck = FindFolderWithId(rootTree, newParentId);

            var success = false;

            if (!folderToCheck.IsPrivate)
            {
                success = true;
            }

            return success;
        }

        public async Task<FolderServiceModel> GetFolderByIdAsync(int folderId, int employeeId)
        {
            var folder = await this.folderDAL.GetFolderByIdAsync(folderId);

            if (folder is null)
            {
                Guard.AgainstNullObject<FolderException>(folder, nameof(folder));
            }

            var rootId = folder.RootId;

            if (rootId is null)
            {
                rootId = folder.FolderId;
            }

            var folderToReturn = await this.GenerateFolderWithTreeAsync(rootId.Value, employeeId, folderId);

            folderToReturn.Files = await fileDAL.GetFilesByFolderIdAsync(folderId);

            foreach (var file in folderToReturn.Files)
            {
                file.UpdaterUsername = await this.employeeService.GetEmailByIdAsync(file.UpdaterId);
            }

            await this.publisher.Publish(new FolderOpenedMessage
            {
                FolderId = folder.FolderId,
                UserId = await this.employeeService.GetUserIdByEmployeeAsync(employeeId)
            });

            return folderToReturn;
        }

        public async Task<IReadOnlyCollection<FolderServiceModel>> GetFoldersByCatalogIdAsync(int catalogId, int employeeId)
        {
            var userFolderPermissions = await this.permissionsDAL.GetUserFolderPermissionsAsync(catalogId, employeeId);
            var folders = await this.folderDAL.GetFoldersByCatalogIdAsync(catalogId);

            if (folders != null && folders.Count() < 1)
            {
                return null;
            }

            foreach (var folder in folders)
            {
                folder.SubFolders = await this.GetSubFoldersAsync(folder.FolderId, userFolderPermissions);
            }

            return folders.ToList();
        }

        public Task<bool> IsFolderPrivateAsync(int folderId)
            => folderDAL.IsFolderPrivateAsync(folderId);

        public async Task<FolderServiceModel> GetRootFolderByCatalogIdAsync(
            int catalogId, 
            int employeeId,
            bool includeSubfolders = true)
        {
            var root = await this.folderDAL.GetRootFolderByCatalogIdAsync(catalogId);

            if (root is null)
            {
                return null;
            }

            FolderServiceModel folder;

            if (includeSubfolders)
            {
                folder = await this.GenerateFolderWithTreeAsync(root.FolderId, employeeId, root.FolderId);
            }
            else
            {
                folder = root;
            }

            folder.Files = await this.fileDAL.GetFilesByFolderIdAsync(folder.FolderId);

            foreach (var file in folder.Files)
            {
                file.UpdaterUsername = await this.employeeService.GetEmailByIdAsync(file.UpdaterId);
            }

            await this.publisher.Publish(new FolderOpenedMessage
            {
                FolderId = folder.FolderId,
                UserId = await this.employeeService.GetUserIdByEmployeeAsync(employeeId)
            });

            return folder;
        }

        public async Task<FolderServiceModel> GetAccessableFolders(int catalogId, int employeeId)
        {
            var root = await this.folderDAL.GetRootFolderByCatalogIdAsync(catalogId);

            var folderTree = await this.folderDAL.GetFolderTreeAsync(root.FolderId, employeeId);
            var rootTree = this.MapToFolderModel(folderTree.First(f => f.ParentId is null && f.RootId is null));

            var foldersToFindSubfoldersFor = new List<FolderServiceModel> { rootTree };

            while (foldersToFindSubfoldersFor.Any())
            {
                var folder = foldersToFindSubfoldersFor.First();
                folder.SubFolders = folderTree
                    .Where(f => f.HasAccess && f.ParentId == folder.FolderId)
                    .Select(MapToFolderModel).ToList();

                foldersToFindSubfoldersFor.Remove(folder);
                foldersToFindSubfoldersFor.AddRange(folder.SubFolders);
            }

            return rootTree;
        }

        public async Task<bool> DeleteFolderAsync(int catalogId, int employeeId, int folderId)
        {
            var existingFolder = await this.folderDAL.GetFolderByIdAsync(folderId);
            return await this.DeleteFolderOperationsAsync(catalogId, employeeId, existingFolder);
        }

        public Task<int> CountFoldersAsync(int employeeId)
            => this.folderDAL.CountFoldersForEmployeeAsync(employeeId);

        public Task<IReadOnlyCollection<OutputFolderFlatServiceModel>> GetAllForEmployeeAsync(int employeeId)
            => this.folderDAL.GetAllFlatForEmployeeAsync(employeeId);

        private async Task<bool> DeleteFolderOperationsAsync(int catalogId, int employeeId, FolderServiceModel existingFolder)
        {
            try
            {
                await this.ValidateFolderAndCheckPermissionsAsync(catalogId, employeeId, existingFolder);
                await this.AttachSubFoldersAsync(catalogId, employeeId, existingFolder);
                await this.DeleteFilesAsync(catalogId, employeeId, existingFolder);
                await this.DeleteFolderAndSubFoldersRecursivelyAsync(catalogId, employeeId, existingFolder);

                return true;
            }
            catch (FolderException)
            {
                throw;
            }
            catch (PermissionException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        private async Task DeleteFolderAndSubFoldersRecursivelyAsync(int catalogId, int employeeId, FolderServiceModel folder)
        {
            // Cant delete the root folder
            if (folder is null || !folder.RootId.HasValue)
            {
                return;
            }

            // folders must be empty at this point
            var isDeleted = await this.folderDAL.DeleteAsync(catalogId, folder.FolderId);
            if (!isDeleted)
            {
                return;
            }

            foreach (var subFolder in folder.SubFolders)
            {
                await this.DeleteFolderAndSubFoldersRecursivelyAsync(catalogId, employeeId, subFolder);
            }
        }

        private async Task AttachSubFoldersAsync(int catalogId, int employeeId, FolderServiceModel folder)
        {
            folder.SubFolders = await this.folderDAL.GetSubFoldersAsync(folder.FolderId);
            await this.AttachSubFoldersRecursivelyAsync(catalogId, employeeId, folder.SubFolders);
        }

        private async Task AttachSubFoldersRecursivelyAsync(int catalogId, int employeeId, IEnumerable<FolderServiceModel> folders)
        {
            foreach (var folder in folders)
            {
                // skip if you dont have access
                if (folder.IsPrivate)
                {
                    var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catalogId, folder.FolderId, employeeId);
                    if (!hasPermission)
                    {
                        continue;
                    }
                }

                folder.SubFolders = await this.folderDAL.GetSubFoldersAsync(folder.FolderId);
                await this.AttachSubFoldersRecursivelyAsync(catalogId, employeeId, folder.SubFolders);
            }
        }

        private async Task DeleteFilesAsync(int catalogId, int employeeId, FolderServiceModel parentFolder)
        {
            if (parentFolder is null)
            {
                return;
            }

            await this.DeleteFilesForFolderAsync(parentFolder.FolderId);

            foreach (var subFolder in parentFolder.SubFolders)
            {
                await this.DeleteFilesForFolderAsync(subFolder.FolderId);
                await this.DeleteFilesAsync(catalogId, employeeId, subFolder);
            }
        }

        private async Task DeleteFilesForFolderAsync(int folderId)
        {
            var files = await this.fileDAL.GetFilesByFolderIdAsync(folderId);

            foreach (var file in files)
            {
                await this.fileDAL.DeleteFileAsync(file.CatalogId, file.FolderId, file.FileId, file.BlobId);
            }
        }

        private async Task ValidateFolderAndCheckPermissionsAsync(int catalogId, int employeeId, FolderServiceModel folder)
        {
            if (folder is null)
            {
                throw new FolderException { Message = "The folder does not exist." };
            }

            if (folder.IsPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(
                    catalogId, 
                    folder.FolderId, 
                    employeeId);

                if (!hasPermission)
                {
                    throw new PermissionException { Message = "You dont have access to this folder." };
                }
            }
        }

        private async Task<IEnumerable<FolderServiceModel>> GetSubFoldersAsync(
            int folderId, 
            IEnumerable<int> permissions)
            => (await this.folderDAL.GetSubFoldersAsync(folderId))
                .Where(x => !x.IsPrivate || (x.IsPrivate && permissions.Contains(x.FolderId)));

        private void ValidateInput(InputFolderServiceModel model)
        {
            Guard.AgainstNullObject<FolderException>(model, nameof(model));
            Guard.AgainstEmptyString<FolderException>(model.Name, nameof(model.Name));
            Guard.AgainstInvalidWindowsCharacters<FolderException>(model.Name, nameof(model.Name));
            Guard.AgainstLessThanOne<FolderException>(model.CatalogId, nameof(model.CatalogId));
            Guard.AgainstLessThanOne<FolderException>(model.EmployeeId, nameof(model.EmployeeId));

            if (model.IsPrivate && (model.RootId is null))
            {
                throw new FolderException { Message = "The folder is private." };
            }
        }

        private async Task ValidateParentAndRootAsync(InputFolderServiceModel model)
        {
            if (model.ParentId != null && model.RootId != null)
            {
                var parentModelTask = this.folderDAL.GetFolderByIdAsync(model.ParentId.Value);
                var rootModelTask = this.folderDAL.GetFolderByIdAsync(model.RootId.Value);

                await Task.WhenAll(parentModelTask, rootModelTask);

                var parentModel = await parentModelTask;
                var rootModel = await rootModelTask;

                if (parentModel is null)
                {
                    throw new FolderException { Message = "The parent folder does not exist." };
                }

                if (rootModel is null)
                {
                    throw new FolderException { Message = "The root folder does not exist" };
                }

                if (parentModel.CatalogId != model.CatalogId || rootModel.CatalogId != model.CatalogId)
                {
                    throw new FolderException { Message = "Different catalogs." };
                }

                var folderWithSameNameExist = await folderDAL.CheckForFolderWithSameNameAsync(model.Name, model.ParentId.Value);
                if (folderWithSameNameExist)
                {
                    throw new FolderException { Message = "Parentfolder already contains a folder with that name." };
                }
            }
        }

        private FolderServiceModel MapToFolderModel(FolderWithAccessServiceModel folder)
        {
            return new FolderServiceModel
            {
                CatalogId = folder.CatalogId,
                EmployeeId = folder.EmployeeId,
                FolderId = folder.FolderId,
                IsPrivate = folder.IsPrivate,
                Name = folder.Name,
                ParentId = folder.ParentId,
                ParentName = folder.ParentName,
                RootId = folder.RootId,
                FileCount = folder.FileCount,
                FolderCount = folder.FolderCount
            };
        }

        private void CalculateTotalFileCountAndFolderCountForTree(FolderServiceModel folder)
        {
            if (folder.SubFolders.Any())
            {
                foreach (var subFolder in folder.SubFolders)
                {
                    CalculateTotalFileCountAndFolderCountForTree(subFolder);
                }

                folder.FileCount += folder.SubFolders.Sum(f => f.FileCount);
                folder.FolderCount += folder.SubFolders.Sum(f => f.FolderCount);
            }
        }

        private async Task<FolderServiceModel> GenerateFolderWithTreeAsync(int rootFolderId, int employeeId, int folderId)
        {
            var folderTree = await this.folderDAL.GetFolderTreeAsync(rootFolderId, employeeId);
            var rootTree = this.MapToFolderModel(folderTree.First(f => f.ParentId is null && f.RootId is null));

            var foldersToFindSubfoldersFor = new List<FolderServiceModel> { rootTree };

            while (foldersToFindSubfoldersFor.Any())
            {
                var folder = foldersToFindSubfoldersFor.First();
                folder.SubFolders = folderTree
                    .Where(f => f.HasAccess && f.ParentId == folder.FolderId)
                    .Select(MapToFolderModel).ToList();

                foldersToFindSubfoldersFor.Remove(folder);
                foldersToFindSubfoldersFor.AddRange(folder.SubFolders);
            }

            this.CalculateTotalFileCountAndFolderCountForTree(rootTree);

            var folderToReturn = FindFolderWithId(rootTree, folderId);

            folderToReturn.Files = await this.fileDAL.GetFilesByFolderIdAsync(folderToReturn.FolderId);

            foreach (var subfolder in folderToReturn.SubFolders)
            {
                subfolder.SubFolders = Array.Empty<FolderServiceModel>();
            }

            return folderToReturn;
        }

        private FolderServiceModel FindFolderWithId(FolderServiceModel folder, int folderId)
        {
            if (folder.FolderId == folderId)
            {
                return folder;
            }
            else if (!folder.SubFolders.Any())
            {
                return null;
            }
            else
            {
                foreach (var subFolder in folder.SubFolders)
                {
                    var foundFolder = FindFolderWithId(subFolder, folderId);
                    if (foundFolder != null)
                    {
                        return foundFolder;
                    }
                }

                return null;
            }
        }
    }
}
