import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { DriveService } from 'src/app/core/drive.service';
import { NotificationService } from 'src/app/core/notification.service';
import { AuthService } from 'src/app/core/auth.service';
import * as plupload from 'plupload';
import { Folder } from './folder.model';
import { FileModel } from './file.model';
import { FolderIdName } from './folder-id-name.model';
import { SignalRService } from 'src/app/core/signalR.service';
import { FaIconPipe } from 'src/app/core/fa-icons.pipe';
import { EmployeeService } from 'src/app/core/employee.service';

@Component({
  selector: 'app-drive',
  templateUrl: './drive.component.html',
  styleUrls: ['./drive.component.scss'],
  providers: [FaIconPipe]
})
export class DriveComponent implements OnInit {
  private readonly FILE_MAX_SIZE = 2147483647;

  // PLUPLOAD
  // @ViewChild('pluploader') element: ElementRef;
  // @ViewChild('pluploadcontainer') containerElement: ElementRef;
  public dropElement: any;
  public uploader: plupload.Uploader;
  parameters: object;
  //

  public companyDepartmentsId: number;
  public selectedFolderId: number;
  public folder: Folder;
  public newFolderName: string;
  public creatingFolder: boolean;
  public creatingFile: boolean;
  public selectedFolder: Folder;
  public folderToMove: Folder;
  public selectedFile: FileModel;
  public newFileName: string;
  public moveFileFolders: Map<string, number>[] = [];
  public moveFileToFolder: number;
  public fileToMove: FileModel;
  public parentFolderChain: FolderIdName[] = [];
  public isLoading = true;

  //search
  @ViewChild('searchValue', {static: true}) searchValue: ElementRef;
  public hasSearchResult = false;
  public searchResults: FileModel[] = [];
  //

  public constructor(
    private readonly driveService: DriveService,
    private readonly notificationService: NotificationService,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly signalRService: SignalRService,
    private readonly faIconPipe: FaIconPipe,
    private readonly employeeService: EmployeeService) {
      this.router.routeReuseStrategy.shouldReuseRoute = () => false;
  }

  public ngOnInit(): void {
    this.employeeService.getCompanyDepartmentsSignId()
      .subscribe(id => {
        if (id < 1) {
          this.notificationService.errorMessage('Please pick company/department from your profile!');
        } else {
          this.companyDepartmentsId = id;
          this.selectedFolderId = Number(this.route.snapshot.paramMap.get('selectedFolderId'));

          this.signalRService.subscribe();

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
      });
  }

  // ngAfterViewInit() {
  //   if (this.uploader) {
  //     this.uploader.settings.multipart_params = this.getGroupParameters();
  //   } else {
  //     this.uploader = this.initPlupload();
  //   }
  // }

  public navigateToRoot(){
    this.router.navigate(['drive', this.companyDepartmentsId]);
  }

  public getRootFolder(): void {
    this.isLoading = true;

    this.driveService.getRootFolder(this.companyDepartmentsId)
      .subscribe(folder => {

        this.folder = folder;
        this.addFolderToParentFolderChain(folder.folderId, folder.name);
        this.isLoading = false;
      }, error => {
        this.isLoading = false;
      });
  }

  private getFolder(folderId: number): void {
    this.isLoading = true;

    this.driveService.getFolder(folderId, this.companyDepartmentsId)
      .subscribe(folder => {

        this.folder = folder;
        this.addFolderToParentFolderChain(folder.folderId, folder.name);
        this.isLoading = false;
      }, error => {
        this.isLoading = false;
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

  getFontAwesomeIconName(fileType: string): string {
    return this.faIconPipe.transform(fileType) + ' pr-3';
  }

  // FOLDER ACTIONS
  public openFolder(folderId: number): void {
    this.selectedFolderId = folderId;
    this.router.navigate(['drive', this.companyDepartmentsId, this.selectedFolderId]);
  }

  public stopPropagation(event: MouseEvent) {
    event.stopPropagation();
  }

  public deleteFolder(folder: Folder) {
    const confirmation = confirm('Confirm delete of ' + folder.name);

    if (confirmation) {
      this.driveService.deleteFolder(folder.catalogId, folder.folderId)
        .subscribe(res => {
          if (res) {
            this.reloadFolder();
          }
        }, error => console.log(error));
    }
  }

  public createFolder() {
    this.creatingFolder = true;
    this.newFolderName = 'Default folder';

    this.driveService.createFolder(this.folder, this.newFolderName)
      .subscribe(_ => {
        this.reloadFolder();
        this.newFolderName = '';
        this.creatingFolder = false;
      }, error => this.creatingFolder = false);
    // if (!this.newFolderName || this.newFolderName.length < 1) {
    //   this.notificationService.errorMessage('Empty name');
    // } else if (this.newFolderName.length > 50) {
    //   this.notificationService.errorMessage('The name is too long');
    // } else {
    // }
  }

  // FILE ACTIONS
  public downloadFile(file: FileModel, shouldOpen: boolean = false) {
    const downloadUrl = this.driveService.downloadFile(file.catalogId, file.folderId, file.fileId, shouldOpen);
    window.open(downloadUrl);
  }

  public deleteFile(file: FileModel) {
    const confirmation = confirm('Confirm delete of ' + file.fileName);

    if (confirmation) {
      this.driveService.deleteFile(file.catalogId, file.folderId, file.fileId)
        .subscribe(res => {
          if (res) {
            this.reloadFolder();
          } else {
            console.log('Could not remove the data');
          }
      }, error => console.log(error));
    }
  }

  // PLUPLOAD
  // initPlupload() {
  //   this.parameters = this.getGroupParameters();

  //   const uploader = new plupload.Uploader({
  //     runtimes: 'html5',
  //     browse_button: this.element.nativeElement,
  //     drop_element: 'app-body',
  //     container: this.containerElement.nativeElement,
  //     multipart_params: this.parameters,
  //     chunk_size: '10mb',
  //     url: environment.driveUrl + '/files/UploadFileToFolder',
  //     headers: { Authorization: 'Bearer ' + this.authService.getToken() },
  //     filters: { prevent_empty: false} as any
  //   });

  //   uploader.init();

  //   uploader.bind('FilesAdded', (up, files) => {
  //     const fileNamesAdded = [];

  //     files.forEach(element => {
  //       if (element.size > this.FILE_MAX_SIZE) {
  //         up.removeFile(element);

  //         if (up.state === plupload.STARTED && element.status === plupload.UPLOADING) {
  //           up.stop();
  //           up.start();
  //         }

  //         this.notificationService.errorMessage('File max size is 2GB');
  //         return;
  //       }

  //       fileNamesAdded.push(element.name.toString());
  //     });

  //     this.driveService.checkFilesNamesForFolder(this.folder.catalogId, this.folder.folderId, fileNamesAdded)
  //       .subscribe(response => {
  //         const fileNamesToBeRenamed = [];

  //         for (const key in response) {
  //           const value = response[key];
  //           if (value) {
  //             fileNamesToBeRenamed.push(key);
  //           }
  //         }

  //         if (fileNamesToBeRenamed.length > 0) {
  //           const confirmation = confirm('Some files already exist in the folder, do you want to replace them?');
  //           if (confirmation) {
  //               uploader.settings.multipart_params.replaceExistingFiles = true;
  //               document.getElementById('upload-overlay').style.width = '100%';
  //               uploader.start();
  //           }
  //           // ask user to choose from -> overwrite files / rename files / cancel upload
  //           // and tell user what files clashes

  //           // set users chocie like this
  //           // uploader.settings.multipart_params.GOOD_override_name = true/false;

  //           // end with this
  //           // document.getElementById('upload-overlay').style.width = '100%';
  //           // uploader.start();
  //         } else {
  //           document.getElementById('upload-overlay').style.width = '100%';
  //           uploader.start();
  //         }
  //       });
  //   });

  //   uploader.bind('UploadComplete', (up, file) => {
  //     setTimeout(() => {
  //       this.uploader.files.length = 0;
  //       document.getElementById('upload-overlay').style.width = '0';
  //       this.reloadFolder();
  //     }, 500);
  //   });

  //   uploader.bind('BeforeChunkUpload', (up: plupload.Uploader, file: any, post: any) => {
  //     // Send the total file size
  //     post.total_filesize = file.size;
  //   });

  //   return uploader;
  // }

  getGroupParameters(): object {
    if (this.folder) {
      return {
        catalogId: this.folder.catalogId.toString(),
        folderId: this.folder.folderId.toString()
      };
    }
  }

  searchForFiles(e) {
    e.preventDefault();

    const val = this.searchValue.nativeElement.value;
    if (val.length > 0) {
      this.driveService.searchForFile(this.folder.catalogId, this.folder.folderId, val)
        .subscribe(result => {
          this.hasSearchResult = true;
          this.searchResults = result;
        });
    } else {
      this.hasSearchResult = false;
    }
  }

  updateSearch() {
    const val = this.searchValue.nativeElement.value;
    if (val.length === 0) {
      this.hasSearchResult = false;
    }
  }

  createNewDocumentFile(isWord: boolean) {
    if (isWord) {
      this.driveService.createNewFile(this.folder.catalogId, this.folder.folderId, 1)
        .subscribe(_ => {
          this.reloadFolder();
        });
    } else {
      this.driveService.createNewFile(this.folder.catalogId, this.folder.folderId, 2)
        .subscribe(_ => {
          this.reloadFolder();
        });
    }
  }

  pluploadHell(): void {
    this.notificationService.warningMessage('Plupload needs to be adjusted, now does not work.');
  }

  notImplementedYet(): void {
    this.notificationService.warningMessage('Not implemented yet.');
  }
}