import { Router, ActivatedRoute } from '@angular/router';
import { Component, Input, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { DriveService } from 'src/app/core/drive.service';
import { NotificationService } from 'src/app/core/notification.service';
import { AuthService } from 'src/app/core/auth.service';
import * as plupload from 'plupload';
import { Folder } from './folder.model';
import { FileModel } from './file.model';
import { FolderIdName } from './folder-id-name.model';
import { environment } from 'src/environments/environment';
import { StatisticsService } from 'src/app/core/statistics.service';

@Component({
  selector: 'app-drive',
  templateUrl: './drive.component.html',
  styleUrls: ['./drive.component.scss']
})
export class DriveComponent implements OnInit, AfterViewInit {
  private readonly FILE_MAX_SIZE = 2147483647;

  // PLUPLOAD
  @ViewChild('pluploader') element: ElementRef;
  @ViewChild('pluploadcontainer') containerElement: ElementRef;
  public fileUploadText = 'Upload';
  public dropElement: any;
  public uploader: plupload.Uploader;
  parameters: object;
  //

  public companyId: number = 1;
  public departmentId: number = 1;
  public selectedFolderId: number;

  public folder: Folder;
  public createFolderFlyoutOpen: boolean;
  public moveFileFlyoutOpen: boolean;
  public moveFolderFlyoutOpen: boolean;
  public newFolderName: string;
  public creatingFolder: boolean;
  public creatingFile: boolean;
  public renameFolderFlyoutOpen: boolean;
  public selectedFolder: Folder;
  public folderToMove: Folder;
  public selectedFile: FileModel;
  public newFileName: string;
  public moveFileFolders: Map<string, number>[] = [];
  public moveFileToFolder: number;
  public fileToMove: FileModel;
  public renameFileFlyoutOpen: boolean;
  public folderDetailsFlyoutOpen: boolean;
  public fileDetailsFlyoutOpen: boolean;
  public parentFolderChain: FolderIdName[] = [];
  public isLoading = true;

  //search
  @ViewChild('searchValue', {static: true}) searchValue: ElementRef;
  public hasSearchResult = false;
  public searchResults: FileModel[] = [];
  //

  public constructor(
    private readonly driveService: DriveService,
    private readonly statisticsService: StatisticsService,
    private readonly notificationService: NotificationService,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute) {
      this.router.routeReuseStrategy.shouldReuseRoute = () => false;
  }

  public ngOnInit(): void {
    const params = this.route.snapshot;
    this.companyId = Number(params.paramMap.get('companyId'));
    this.departmentId = Number(params.paramMap.get('departmentId'));
    this.selectedFolderId = Number(params.paramMap.get('selectedFolderId'));

    this.route.data.subscribe(data => {
      switch (data.kind) {
        case 'root':
          this.getRootFolder();
          break;
        case 'folder':
          this.getFolder(this.selectedFolderId);
          break;
      }});
  }

  ngAfterViewInit() {
    if (this.uploader) {
      this.uploader.settings.multipart_params = this.getGroupParameters();
    } else {
      this.uploader = this.initPlupload();
    }
  }

  public navigateToRoot(){
    this.router.navigate(['drive', this.companyId, this.departmentId]);
  }

  public getRootFolder(): void {
    this.isLoading = true;

    this.driveService.getRootFolder(this.companyId, this.departmentId)
      .subscribe(folder => {
        if (folder) {
          this.statisticsService.addFolderView(folder.folderId)
            .subscribe();
        }

        this.folder = folder;
        this.addFolderToParentFolderChain(folder.folderId, folder.name);
        this.isLoading = false;
      }, error => {
        this.isLoading = false;
      });
  }

  private getFolder(folderId: number): void {
    this.isLoading = true;

    this.driveService.getFolder(folderId)
      .subscribe(folder => {
        if (folder) {
          this.statisticsService.addFolderView(folder.folderId)
            .subscribe();
        }

        this.folder = folder;
        this.addFolderToParentFolderChain(folder.folderId, folder.name);
        this.isLoading = false;
      }, error => {
        this.navigateToRoot();
      });
  }

  public reloadFolder(): void {
    if (this.folder) {
      if (this.folder.rootId) {
        this.getFolder(this.folder.folderId);
      } else {
        this.getRootFolder();
      }
    }
  }

  private addFolderToParentFolderChain(folderId: number, name: string): void {
    this.removeFolderFromTheParentFolderChain(folderId);
    this.parentFolderChain.push(new FolderIdName(folderId, name));
  }

  private removeFolderFromTheParentFolderChain(folderId: number): void {
    const parentIndex = this.parentFolderChain.findIndex(f => f.id === folderId);

    if (parentIndex >= 0) {
      this.parentFolderChain = this.parentFolderChain.slice(0, parentIndex);
    } else {
      this.parentFolderChain = [...this.parentFolderChain];
    }
  }

  // FOLDER ACTIONS
  public openFolder(folderId: number): void {
    this.selectedFolderId = folderId;
    this.router.navigate(['drive', this.companyId, this.departmentId, this.selectedFolderId]);
  }

  public openCreateFolder() {
    this.newFolderName = '';
    this.createFolderFlyoutOpen = true;
  }

  public stopPropagation(event: MouseEvent) {
    event.stopPropagation();
  }

  public openRenameFolder(folder: Folder) {
    this.selectedFolder = folder;
    this.newFolderName = folder.name;
    this.renameFolderFlyoutOpen = true;
  }

  public seeFolderDetails(folder: Folder) {
    this.folderDetailsFlyoutOpen = true;
    this.selectedFolder = folder;
  }

  public openMoveFolder(folder: Folder) {
    this.folderToMove = folder;
    this.moveFolderFlyoutOpen = true;
  }

  public openDeleteFolder(folder: Folder) {
    let confirmation = confirm("Confirm delete of " + folder.name);

    if (confirmation) {
      this.driveService.deleteFolder(folder.catalogId, folder.folderId)
        .subscribe(res => {
          if (res) {
            this.reloadFolder();
          } else {
            console.log("Could not remove the data.");
          }
        }, error => console.log(error));
    }
  }

  public createFolder() {
    if (!this.newFolderName || this.newFolderName.length < 1) {
      this.notificationService.errorMessage("Empty name");
    } else if (this.newFolderName.length > 50) {
      this.notificationService.errorMessage("The name is too long");
    } else {
      this.creatingFolder = true;
      this.driveService.createFolder(this.folder, this.newFolderName)
        .subscribe(response => {
          this.reloadFolder();
          this.newFolderName = '';
          this.createFolderFlyoutOpen = false;
          this.creatingFolder = false;
        }, error => this.creatingFolder = false);
    }
  }

  public renameFolder() {
    if (!this.newFolderName || this.newFolderName.length < 1) {
      this.notificationService.errorMessage("Empty name");
    } else if (this.newFolderName.length > 50) {
      this.notificationService.errorMessage("The name is too long");
    } else {
      this.creatingFolder = true;
      this.driveService.renameFolder(this.selectedFolder, this.newFolderName)
        .subscribe(response => {
          this.newFolderName = '';
          this.renameFolderFlyoutOpen = false;
          this.creatingFolder = false;
          this.reloadFolder();
        }, error => this.creatingFolder = false);
    }
  }

  // FILE ACTIONS
  public downloadFile(file: FileModel, shouldOpen: boolean = false) {
    const downloadUrl = this.driveService.downloadFile(file.catalogId, file.folderId, file.fileId, shouldOpen);
    window.open(downloadUrl);
  }

  public openMoveFile(file: FileModel) {
    this.fileToMove = file;
    this.moveFileFlyoutOpen = true;
  }

  public deleteFile(file: FileModel) {
    let confirmation = confirm("Confirm delete of " + file.fileName);

    if (confirmation) {
      this.driveService.deleteFile(file.catalogId, file.folderId, file.fileId)
        .subscribe(res => {
          if (res) {
            this.reloadFolder();
          } else {
            console.log("Could not remove the data");
          }
      }, error => console.log(error));
    }
  }

  public openRenameFile(file: FileModel) {
    this.selectedFile = file;
    this.newFileName = file.fileName;
    this.renameFileFlyoutOpen = true;
  }

  public renameFile() {
    if (!this.newFileName || this.newFileName.length < 1) {
      this.notificationService.errorMessage("Empty file name");
    } else if (this.newFileName.length > 50) {
      this.notificationService.errorMessage("The file name is too long");
    } else {
      this.creatingFile = true;
      this.driveService.renameFile(this.selectedFile, this.newFileName)
        .subscribe(response => {
          this.newFileName = '';
          this.renameFileFlyoutOpen = false;
          this.creatingFile = false;
          this.reloadFolder();
        }, error => this.creatingFile = false);
    }
  }

  public fileDetails(file: FileModel) {
    this.fileDetailsFlyoutOpen = true;
    this.selectedFile = file;
  }

  // PLUPLOAD
  // PLUPLOAD
  initPlupload() {
    this.parameters = this.getGroupParameters();

    const uploader = new plupload.Uploader({
      runtimes: 'html5',
      browse_button: this.element.nativeElement,
      drop_element: 'app-body',
      container: this.containerElement.nativeElement,
      multipart_params: this.parameters,
      chunk_size: '10mb',
      url: environment.driveUrl + '/files/UploadFileToFolder',
      headers: { Authorization: 'Bearer ' + this.authService.getToken() },
      filters: { prevent_empty: false} as any
    });

    uploader.init();

    uploader.bind('FilesAdded', (up, files) => {
      const fileNamesAdded = [];

      files.forEach(element => {
        if (element.size > this.FILE_MAX_SIZE) {
          up.removeFile(element);

          if(up.state === plupload.STARTED && element.status === plupload.UPLOADING)
          {
            up.stop();
            up.start();
          }

          this.notificationService.errorMessage("File max size is 2GB");
          return;
        }

        fileNamesAdded.push(element.name.toString());
      });

      this.driveService.checkFilesNamesForFolder(this.folder.catalogId, this.folder.folderId, fileNamesAdded)
        .subscribe(response => {
          const fileNamesToBeRenamed = [];

          for (const key in response) {
            const value = response[key];
            if (value) {
              fileNamesToBeRenamed.push(key);
            }
          }

          if (fileNamesToBeRenamed.length > 0) {
            const confirmation = confirm("Some files already exist in the folder, do you want to replace them?");
            if (confirmation) {
                uploader.settings.multipart_params.replaceExistingFiles = true;
                document.getElementById('upload-overlay').style.width = '100%';
                uploader.start();
            }
            // ask user to choose from -> overwrite files / rename files / cancel upload
            // and tell user what files clashes

            // set users chocie like this
            // uploader.settings.multipart_params.GOOD_override_name = true/false;

            // end with this
            // document.getElementById('upload-overlay').style.width = '100%';
            // uploader.start();
          } else {
            document.getElementById('upload-overlay').style.width = '100%';
            uploader.start();
          }
        });
    });

    uploader.bind('UploadComplete', (up, file) => {
      setTimeout(() => {
        this.uploader.files.length = 0;
        document.getElementById('upload-overlay').style.width = '0';
        this.reloadFolder();
      }, 500);
    });

    uploader.bind('BeforeChunkUpload', (up: plupload.Uploader, file: any, post: any) => {
      // Send the total file size
      post.total_filesize = file.size;
    });

    return uploader;
  }

  getGroupParameters(): object {
    if (this.folder) {
      return {
        catalogId: this.folder.catalogId.toString(),
        folderId: this.folder.folderId.toString()
      };
    }
  }

  getFontAwesomeIconName(fileType: string): string {
    //return this.faIconPipe.transform(fileType) + " pr-3";
    return '';
  }


  searchForFiles(e){
    e.preventDefault();

    const val = this.searchValue.nativeElement.value;
    if (val.length > 0) {
      this.driveService.searchForFile(this.folder.catalogId, val)
        .subscribe(result => {
          if (!result.length) {
            this.hasSearchResult = false;
            this.searchResults = result;
          } else {
            this.hasSearchResult = true;
            this.searchResults = result;
          }
        });
    }
  }

  updateSearch() {
    const val = this.searchValue.nativeElement.value;
    if (val.length === 0) {
      this.hasSearchResult = false;
    }
  }
}
