﻿using System.IO.Compression;
using System.Security.Cryptography;
using EnterpriseCoder.MartenDb.GridFs.DtoMapping;
using EnterpriseCoder.MartenDb.GridFs.Entities;
using EnterpriseCoder.MartenDb.GridFs.Procedures;
using EnterpriseCoder.MartenDb.GridFs.Utility;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.MartenDb.GridFs;

public class GridFileSystem : IGridFileSystem
{
    private const int FileBlockSize = 65535;

    private readonly GridFileHeaderProcedures _fileHeaderProcedures = new();
    private readonly GridFileBlockProcedures _fileBlockProcedures = new();

    public async Task UploadStreamAsync(IDocumentSession documentSession, GridFsFilePath filePath, Stream inStream,
        bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L)
    {
        if (overwriteExisting is false)
        {
            if (await FileExistsAsync(documentSession, filePath))
            {
                throw new IOException(
                    $"File {filePath} already exists and {nameof(overwriteExisting)} is set to false.");
            }
        }

        // No overwrite protection...call delete to make sure 
        // there's nothing taking the incoming filePath resource name.
        await DeleteFileAsync(documentSession, filePath);

        // Create a temp file stream to save the incoming stream into...
        using TemporaryFilenameDisposable tempFilename = new TemporaryFilenameDisposable();
        byte[] buffer = new byte[FileBlockSize];
        byte[] sha256Hash;

        long originalFileSize = 0L;
        using (var sha256 = SHA256.Create())
        {
            using (Stream tempStream = File.OpenWrite(tempFilename.FilePath))
            {
                using (var zipStream = new GZipStream(tempStream, CompressionMode.Compress))
                {
                    // Read from the inStream
                    int readCount;
                    while ((readCount = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        sha256.TransformBlock(buffer, 0, readCount, buffer, 0);

                        originalFileSize += readCount;
                        await zipStream.WriteAsync(buffer, 0, readCount);
                    }

                    zipStream.Close();
                }
            }

            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            sha256Hash = sha256.Hash!;
        }

        // Get file information about the temp file.
        FileInfo fileInfo = new(tempFilename.FilePath);

        // Create a new header document.
        GridFileHeader header = new GridFileHeader()
        {
            // Id is automatically assigned in constructor
            Directory = filePath.Directory,
            FilePath = filePath,
            OriginalLength = originalFileSize,
            StoredLength = fileInfo.Length,
            Sha256 = sha256Hash,
            UserDataLong = userValue,
            UserDataGuid = userGuid ?? Guid.Empty
        };

        await _fileHeaderProcedures.UpsertAsync(documentSession, header);

        // Write out the blocks...
        using (FileStream tempInStream = new FileStream(tempFilename.FilePath, FileMode.Open))
        {
            int sequenceNumber = 1;
            int readCount;
            while ((readCount = await tempInStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                byte[] saveBuffer = new byte[readCount];
                Buffer.BlockCopy(buffer, 0, saveBuffer, 0, readCount);

                GridFileBlock nextBlock = new GridFileBlock()
                {
                    // Id is assigned by constructor
                    ParentFileHeaderId = header.Id,
                    BlockSequenceNumber = sequenceNumber++,
                    BlockData = saveBuffer
                };

                documentSession.Store(nextBlock);
            }
        }
    }

    public async Task<Stream?> DownLoadStreamAsync(IDocumentSession documentSession, GridFsFilePath filePath)
    {
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, filePath);
        if (targetHeader is null)
        {
            throw new FileNotFoundException(filePath);
        }

        string workFilename = Path.GetTempFileName();
        try
        {
            using (FileStream tempStream = File.OpenWrite(workFilename))
            {
                var blockResults = _fileBlockProcedures.Select(documentSession, targetHeader);

                await foreach (var nextBlock in blockResults)
                {
                    await tempStream.WriteAsync(nextBlock.BlockData, 0, nextBlock.BlockData.Length);
                }
            }

            FileStream rereadStream = new AutoDeleteReadFileStream(workFilename);
            GZipStream unzipStream = new GZipStream(rereadStream, CompressionMode.Decompress);

            return unzipStream;
        }
        finally
        {
            try
            {
                File.Delete(workFilename);
            }
            catch
            {
                // ignored
            }
        }
    }

    public async Task<bool> FileExistsAsync(IDocumentSession documentSession, GridFsFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, filePath);
        return targetHeader != null;
    }

    public async Task DeleteFileAsync(IDocumentSession documentSession, GridFsFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, filePath);
        if (targetHeader is null)
        {
            return;
        }

        // Delete all file blocks associated with this header.
        await _fileBlockProcedures.DeleteAsync(documentSession, targetHeader);

        // Delete the header itself.
        await _fileHeaderProcedures.DeleteAsync(documentSession, targetHeader);
    }

    public async Task<GridFsFileInfo?> GetFileInfoAsync(IDocumentSession documentSession, GridFsFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, filePath);
        if (targetHeader == null)
        {
            return null;
        }

        return targetHeader.ToGridFsFileInfoDto();
    }

    public async Task RenameFileAsync(IDocumentSession documentSession, GridFsFilePath oldFilePath,
        GridFsFilePath newFilePath,
        bool overwriteDestination = false)
    {
        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, oldFilePath);
        if (sourceHeader == null)
        {
            throw new FileNotFoundException(oldFilePath);
        }

        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, newFilePath);
        if (targetHeader != null)
        {
            if (!overwriteDestination)
            {
                throw new IOException($"File {newFilePath} exists and {nameof(overwriteDestination)} is set to false.");
            }

            // Delete the file identified by newFilePath
            await DeleteFileAsync(documentSession, newFilePath);
        }

        sourceHeader.FilePath = newFilePath;
        sourceHeader.Directory = newFilePath.Directory;
        documentSession.Store(sourceHeader);
    }

    public async Task CopyFileAsync(IDocumentSession documentSession, GridFsFilePath oldFilePath,
        GridFsFilePath newFilePath,
        bool overwriteDestination = false)
    {
        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, oldFilePath);
        if (sourceHeader == null)
        {
            throw new FileNotFoundException(oldFilePath);
        }

        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, newFilePath);
        if (targetHeader != null)
        {
            if (!overwriteDestination)
            {
                throw new IOException($"File {newFilePath} exists and {nameof(overwriteDestination)} is set to false.");
            }
        }

        // Copy the file
        await using var oldFileStream = await DownLoadStreamAsync(documentSession, oldFilePath);
        if (oldFileStream == null)
        {
            throw new ApplicationException($"Unable to load {nameof(oldFilePath)}");
        }

        await UploadStreamAsync(documentSession, newFilePath, oldFileStream, true);
    }

    public async Task<IList<GridFsFileInfo>> GetFileListingAsync(IDocumentSession documentSession,
        GridFsDirectory directory, int oneBasedPage, int pageSize,
        bool recursive = false)
    {
        string directoryString = directory;

        IQueryable<GridFileHeader> baseQuery = documentSession.Query<GridFileHeader>();
        if (recursive)
        {
            baseQuery = baseQuery.Where(x => x.Directory.StartsWith(directoryString));
        }
        else
        {
            baseQuery = baseQuery.Where(x => x.Directory == directoryString);
        }

        var pagedList = await baseQuery.ToPagedListAsync(oneBasedPage, pageSize);

        return new List<GridFsFileInfo>(pagedList.Select(x => x.ToGridFsFileInfoDto()));
    }
}
