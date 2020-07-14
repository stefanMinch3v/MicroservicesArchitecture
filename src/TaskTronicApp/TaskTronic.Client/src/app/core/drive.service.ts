import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { NotificationService } from './notification.service';
import { Folder } from '../components/drive/folder.model';
import { FileModel } from '../components/drive/file.model';
import { environment } from 'src/environments/environment';
import { AuthService } from './auth.service';

@Injectable()
export class DriveService {
  private readonly DRIVE_FOLDERS = '/folders/';
  private readonly DRIVE_FILES = '/files/';

  constructor(
    private readonly http: HttpClient,
    private readonly notificationService: NotificationService,
    private readonly authService: AuthService) { }

    createFolder(folder: Folder, folderName: string): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}CreateFolder`;

        return this.http.post(url, {}, {
            params: {
                catalogId: folder.catalogId.toString(),
                rootId: folder.rootId === null ? folder.folderId.toString() : folder.rootId.toString(),
                parentFolderId: folder.folderId.toString(),
                name: folderName
            }}).pipe(map((response: boolean) => {
                this.notificationService.successMessage('Folder created');
                return response;
            }));
    }

    renameFolder(folder: Folder, newFolderName: string): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}RenameFolder`;

        return this.http.post(url, {}, {
            params: {
                catalogId: folder.catalogId.toString(),
                folderId: folder.folderId.toString(),
                name: newFolderName
            }
            }).pipe(map((response: boolean) => {
                this.notificationService.successMessage('Folder renamed');
                return response;
            }));
    }

    renameFile(file: FileModel, newFileName: string): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FILES}RenameFile`;

        return this.http.post(url, {}, {
            params: {
                catalogId: file.catalogId.toString(),
                folderId: file.folderId.toString(),
                fileId: file.fileId.toString(),
                name: newFileName
            }}).pipe(map((response: boolean) => {
                this.notificationService.successMessage('File renamed');
                return response;
            }));
    }

    getRootFolder(companyDepartmentsId: number): Observable<Folder> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}GetRootFolder`;

        return this.http.get(url, {
            params: {
                companyDepartmentsId: companyDepartmentsId.toString()
            }}).pipe(map((response: Folder) => response));
    }

    getFolder(folderId: number): Observable<Folder> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}GetFolderById`;

        return this.http.get(url, {
            params: {
                folderId: folderId.toString()
            }}).pipe(map((response: Folder) => response));
    }

    moveFile(file: FileModel, newFolderId: number): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FILES}MoveFile`;

        return this.http.post(url, {}, {
            params: {
                fileId: file.fileId.toString(),
                catalogId: file.catalogId.toString(),
                folderId: file.folderId.toString(),
                newFolderId: newFolderId.toString()
            }}).pipe(map((response: boolean) => {
                this.notificationService.successMessage('File moved');
                return response;
            }));
    }

    moveFolder(catalogId: number, folderId: number, newFolderId: number): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}MoveFolder`;

        return this.http.post(url, {}, {
            params: {
                catalogId: catalogId.toString(),
                folderId: folderId.toString(),
                newFolderId: newFolderId.toString()
            }}).pipe(map((response: boolean) => {
                this.notificationService.successMessage('Folder moved');
                return response;
            }));
    }

    deleteFolder(catalogId: number, folderId: number): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}DeleteFolder`;

        return this.http.delete(url, {
            params: {
                catalogId: catalogId.toString(),
                folderId: folderId.toString()
            }}).pipe(map((response: boolean) => {
                if (!response) {
                    this.notificationService.errorMessage('Could not remove the data.');
                } else {
                    this.notificationService.successMessage('Folder deleted');
                }

                return response;
            }));
    }

    downloadFile(catalogId: number, folderId: number, fileId: number, shouldOpen: boolean): string {
        const url = environment.driveUrl + `${this.DRIVE_FILES}DownloadFile`;

        const parameters = {
            catalogId: catalogId.toString(),
            folderId: folderId.toString(),
            fileId: fileId.toString(),
            shouldOpen: String(shouldOpen),
            access_token: this.authService.getToken()
        };

        const options = new HttpParams({ fromObject: parameters });
        return `${url}?${options.toString()}`;
    }

    // downloadFileWithoutUrlToken(catalogId: number, folderId: number, fileId: number, shouldOpen: boolean): Observable<Blob> {
    //     const url = environment.driveUrl + `${this.DRIVE_FILES}DownloadFile`;

    //     return this.http.get(url, {
    //         params: {
    //             catalogId: catalogId.toString(),
    //             folderId: folderId.toString(),
    //             fileId: fileId.toString(),
    //             shouldOpen: String(shouldOpen)
    //         }}).pipe(map((response: Blob) => response));
    // }

    deleteFile(catalogId: number, folderId: number, fileId: number): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FILES}DeleteFile`;

        return this.http.delete(url, {
            params: {
                catalogId: catalogId.toString(),
                folderId: folderId.toString(),
                fileId: fileId.toString()
            }}).pipe(map((response: boolean) => {
                this.notificationService.successMessage('File deleted');
                return response;
            }));
    }

    getFileById(catalogId: number, folderId: number, fileId: number): Observable<FileModel> {
        const url = environment.driveUrl + `${this.DRIVE_FILES}GetFileById`;

        return this.http.get(url, {
            params: {
                catalogId: catalogId.toString(),
                folderId: folderId.toString(),
                fileId: fileId.toString()
            }}).pipe(map((response: FileModel) => response));
    }


    searchForFile(catalogId: number, currentFolderId: number, value: string): Observable<Array<FileModel>> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}SearchForFiles`;

        return this.http.get(url, {
            params: {
                catalogId: catalogId.toString(),
                currentFolderId: currentFolderId.toString(),
                searchValue: value
            }}).pipe(map((response: Array<FileModel>) => response));
    }

    checkFilesNamesForFolder(catalogId: number, folderId: number, fileNames: string[]): Observable<Map<string, boolean>> {
        const url = environment.driveUrl + `${this.DRIVE_FOLDERS}CheckFilesNamesForFolder`;

        return this.http.get(url, {
            params: {
                catalogId: catalogId.toString(),
                folderId: folderId.toString(),
                fileNames
            }}).pipe(map((response: Map<string, boolean>) => response));
    }

    createNewFile(catalogId: number, folderId: number, newFileType: number): Observable<boolean> {
        const url = environment.driveUrl + `${this.DRIVE_FILES}CreateNewFile`;

        return this.http.post(url, {}, {
            params: {
                catalogId: catalogId.toString(),
                folderId: folderId.toString(),
                newFileType: newFileType.toString()
            }}).pipe(map((response: boolean) => {
                const message = newFileType === 1 ? 'Word file added' : 'Excel file added';
                this.notificationService.successMessage(message);
                return response;
            }));
    }
}
