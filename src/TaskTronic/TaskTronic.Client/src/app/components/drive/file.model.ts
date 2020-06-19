export interface FileModel {
    fileId: number;
    catalogId: number;
    folderId: number;
    blobId: number;
    filesize: number;
    fileName: string;
    fileType: string;
    contentType: string;
    createDate: Date;
    updateDate: Date;
    updaterUsername: string;
    updaterId?: number;
}