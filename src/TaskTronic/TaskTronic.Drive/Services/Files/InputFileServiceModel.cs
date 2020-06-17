﻿namespace TaskTronic.Drive.Services.Files
{
    using System;
    using System.IO;

    public class InputFileServiceModel : IFileContract
    {
        public int CatalogId { get; set; }
        public int FolderId { get; set; }
        public int BlobId { get; set; }
        public long Filesize { get; set; }
        public string FileName { get; set; }
        public string Revision { get; set; } = string.Empty;
        public string FileType { get; set; }
        public string ContentType { get; set; }
        public string UserId { get; set; }

        public Stream Stream { get; set; }
        public int Chunk { get; set; }
        public int Chunks { get; set; }
        public bool ReplaceExistingFiles { get; set; }

        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset? UpdateDate { get; set; }
    }
}