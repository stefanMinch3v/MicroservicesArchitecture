import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, TemplateRef } from '@angular/core';
import { Location } from '@angular/common';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { DriveService } from 'src/app/core/services/drive.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { AuthService } from 'src/app/core/services/auth.service';
import * as plupload from 'plupload';
import { Folder } from '../../core/models/folder.model';
import { FileModel } from '../../core/models/file.model';
import { FolderIdName } from '../../core/models/folder-id-name.model';
import { SignalRService } from 'src/app/core/services/signalR.service';
import { FaIconPipe } from 'src/app/core/pipes/fa-icons.pipe';
import { EmployeeService } from 'src/app/core/services/employee.service';
import { CommonHelper } from 'src/app/core/helpers/common.helper';

@Component({
  selector: 'app-drive',
  templateUrl: './drive.component.html',
  styleUrls: ['./drive.component.scss'],
  providers: [FaIconPipe]
})
export class DriveComponent implements OnInit {
  // private readonly FILE_MAX_SIZE = 2147483647;
  // PLUPLOAD
  // @ViewChild('pluploader') element: ElementRef;
  // @ViewChild('pluploadcontainer') containerElement: ElementRef;
  // public dropElement: any;
  // public uploader: plupload.Uploader;
  // public parameters: object;
  //

  public companyDepartmentsId: number;
  public selectedFolderId: number;
  public folder: Folder;
  public newFolderName: string;
  public selectedFolder: Folder;
  public newFileName: string;
  private parentArray: FolderIdName[] = [];
  public parentFolderChain: FolderIdName[] = [];
  public isLoading = true;

  // search
  @ViewChild('searchValue', {static: true}) searchValue: ElementRef;
  public hasSearchResult: boolean;
  public searchResults: FileModel[] = [];
  //

  // edit modal
  private modalElementId: number;
  private modalCurrentFolderId: number;
  modalNameToChange: string;
  modalIsFolder: boolean;
  modalRef: BsModalRef;
  @ViewChild('modalTemplate', { static: true }) modalTemplateRef: TemplateRef<any>;

  public constructor(
    private readonly driveService: DriveService,
    private readonly notificationService: NotificationService,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly location: Location,
    private readonly signalRService: SignalRService,
    private readonly faIconPipe: FaIconPipe,
    private readonly employeeService: EmployeeService,
    private readonly modalService: BsModalService) {
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

  public navigateToRoot(): void {
    const url = this.router.createUrlTree(['dok', this.companyDepartmentsId]);
    this.location.go(url.toString());
    this.getRootFolder();
  }

  // Edit file/folder
  public openModal(name: string, isFolder: boolean, currentFolderId: number, elementId: number): void {
    this.modalNameToChange = name;
    this.modalIsFolder = isFolder;
    this.modalElementId = elementId;
    this.modalCurrentFolderId = currentFolderId;

    this.modalRef = this.modalService.show(this.modalTemplateRef);
  }

  public isCurrentUserAuthor(userEmail: string): boolean {
    const emailStartIndex = userEmail.indexOf('@');
    const authorUsername = userEmail.substring(0, emailStartIndex);

    return this.authService.getUser() === authorUsername;
  }

  public getFontAwesomeIconName(fileType: string): string {
    return this.faIconPipe.transform(fileType) + ' pr-3';
  }

  // breadcrumbs actions
  private setUrl(folder: Folder): void {
    this.parentArray = [];
    const url = this.router.createUrlTree(['dok', this.companyDepartmentsId]);
    let newUrl = url.toString();

    if (folder.parentFolder) {
      this.getAllParents(folder);
    } else {
      this.parentArray.push(new FolderIdName(folder.folderId, folder.name));
    }

    this.parentArray = this.parentArray.reverse();
    this.parentFolderChain = this.parentArray;

    this.parentArray.forEach(el => {
      newUrl = newUrl.concat('/', el.name.toString());
    });

    this.location.go(newUrl);
  }

  private getAllParents(folder: Folder): void {
    this.parentArray.push(new FolderIdName(folder.folderId, folder.name));
    if (folder.parentFolder) {
      this.getAllParents(folder.parentFolder);
    }
  }

  // FOLDER actions
  public openFolder(folderId: number): void {
    this.getFolder(folderId);
  }

  public deleteFolder(folder: Folder): void {
    const confirmation = confirm('Confirm delete of ' + folder.name);

    if (confirmation) {
      this.driveService.deleteFolder(folder.catalogId, folder.folderId)
        .subscribe(res => {
          if (res) {
            this.reloadFolder();
          }
        });
    }
  }

  public createFolder(): void {
    this.newFolderName = 'Default folder';

    this.driveService.createFolder(this.folder, this.newFolderName)
      .subscribe(_ => {
        this.reloadFolder();
        this.newFolderName = '';
      });
    // if (!this.newFolderName || this.newFolderName.length < 1) {
    //   this.notificationService.errorMessage('Empty name');
    // } else if (this.newFolderName.length > 50) {
    //   this.notificationService.errorMessage('The name is too long');
    // } else {
    // }
  }

  public togglePrivate(folderId: number, catalogId: number): void {
    this.driveService.togglePrivate(folderId, catalogId)
      .subscribe(_ => this.reloadFolder());
  }

  public renameFolder(): void {
    if (!this.modalNameToChange ||
        this.modalNameToChange.length < 2 ||
        this.modalNameToChange.length > 255) {
      this.notificationService.warningMessage('Name must be at least 2 and maximum 255 symbols long!');
      return;
    }

    const hasInvalidChars = CommonHelper.hasInvalidCharacters(this.modalNameToChange);
    if (hasInvalidChars) {
      this.notificationService.warningMessage('\\,/,:,*,?,<,>,|,\" are not allowed!');
      return;
    }

    this.driveService.renameFolder(this.folder.catalogId, this.modalElementId, this.modalNameToChange)
      .subscribe(_ => {
        this.modalRef.hide();
        this.reloadFolder();
      });
  }

  private getRootFolder(): void {
    this.isLoading = true;
    this.driveService.getRootFolder(this.companyDepartmentsId)
      .subscribe(folder => {
        this.folder = folder;
        this.setUrl(this.folder);
        this.isLoading = false;
      }, error => {
        this.isLoading = false;
      });
  }

  private reloadFolder(): void {
    if (this.folder) {
      if (this.folder.rootId) {
        this.getFolder(this.folder.folderId);
      } else {
        this.getRootFolder();
      }
    }
  }

  private getFolder(folderId: number): void {
    this.isLoading = true;

    this.driveService.getFolder(folderId, this.companyDepartmentsId)
      .subscribe(folder => {

        this.folder = folder;
        this.setUrl(this.folder);
        this.isLoading = false;
      }, error => {
        this.isLoading = false;
        this.navigateToRoot();
      });
  }

  // FILE ACTIONS
  public downloadFile(file: FileModel, shouldOpen: boolean = false): void {
    const downloadUrl = this.driveService.downloadFile(file.catalogId, file.folderId, file.fileId, shouldOpen);
    window.open(downloadUrl);
  }

  public deleteFile(file: FileModel): void {
    const confirmation = confirm('Confirm delete of ' + file.fileName);

    if (confirmation) {
      this.driveService.deleteFile(file.catalogId, file.folderId, file.fileId)
        .subscribe(res => {
          if (res) {
            this.reloadFolder();
          } else {
            this.notificationService.warningMessage('Could not remove the data');
          }
      });
    }
  }

  public searchForFiles(e): void {
    e.preventDefault();
    const val = this.searchValue.nativeElement.value;

    if (val.length > 0) {
      this.isLoading = true;
      this.driveService.searchForFile(this.folder.catalogId, this.folder.folderId, val)
        .subscribe(result => {
          this.hasSearchResult = true;
          this.searchResults = result;
          this.isLoading = false;
        });
    } else {
      this.hasSearchResult = false;
    }
  }

  public updateSearch(): void {
    const val = this.searchValue.nativeElement.value;
    if (val.length === 0) {
      this.hasSearchResult = false;
    }
  }

  public createNewDocumentFile(isWord: boolean): void {
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

  public renameFile(): void {
    if (!this.modalNameToChange ||
        this.modalNameToChange.length < 2 ||
        this.modalNameToChange.length > 255) {
      this.notificationService.warningMessage('Name must be at least 2 and maximum 255 symbols long!');
      return;
    }

    const hasInvalidChars = CommonHelper.hasInvalidCharacters(this.modalNameToChange);
    if (hasInvalidChars) {
      this.notificationService.warningMessage('\\,/,:,*,?,<,>,|,\" are not allowed!');
      return;
    }

    this.driveService.renameFile(this.folder.catalogId, this.folder.folderId, this.modalElementId, this.modalNameToChange)
      .subscribe(_ => {
        this.modalRef.hide();
        this.reloadFolder();
      });
  }

  public pluploadHell(): void {
    this.notificationService.warningMessage('Plupload needs to be adjusted, now does not work.');
  }

  // PLUPLOAD
  // ngAfterViewInit() {
  //   if (this.uploader) {
  //     this.uploader.settings.multipart_params = this.getGroupParameters();
  //   } else {
  //     this.uploader = this.initPlupload();
  //   }
  // }

  // public getGroupParameters(): object {
  //   if (this.folder) {
  //     return {
  //       catalogId: this.folder.catalogId.toString(),
  //       folderId: this.folder.folderId.toString()
  //     };
  //   }
  // }

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
}
