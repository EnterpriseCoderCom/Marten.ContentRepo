using System.IO.Compression;
using System.Security.Cryptography;
using EnterpriseCoder.MartenDb.GridFs.Entities;
using EnterpriseCoder.MartenDb.GridFs.Procedures;
using EnterpriseCoder.MartenDb.GridFs.Utility;
using Marten;

namespace EnterpriseCoder.MartenDb.GridFs;

public class GridFileSystem : IGridFileSystem
{
    private const int BufferSize = 65535;
    
    private readonly IDocumentSession _documentSession;
    private readonly GridFileHeaderProcedures _fileHeaderProcedures = new();
    private readonly GridFileBlockProcedures _fileBlockProcedures = new();
    
    public GridFileSystem(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task SaveStreamAsync(GridFsFilePath filePath, Stream inStream, Guid? userGuid = null,
        long userValue = 0L)
    {
        // If the file already exists, then delete it.
        await DeleteFileAsync(filePath);

        // Create a temp file stream to save the incoming stream into...
        using TemporaryFilenameDisposable tempFilename = new TemporaryFilenameDisposable();
        byte[] buffer = new byte[BufferSize];
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

        await _fileHeaderProcedures.UpsertAsync(_documentSession, header);

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

                _documentSession.Store(nextBlock);
            }
        }
    }

    public async Task<Stream?> LoadStreamAsync(GridFsFilePath filePath)
    {
        var targetHeader = await _fileHeaderProcedures.SelectAsync(_documentSession, filePath);
        if (targetHeader is null)
        {
            throw new FileNotFoundException(filePath);
        }

        string workFilename = Path.GetTempFileName();
        try
        {
            using (FileStream tempStream = File.OpenWrite(workFilename))
            {
                var blockResults = _fileBlockProcedures.Select(_documentSession, targetHeader);

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
    
    public async Task<bool> FileExistsAsync(GridFsFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(_documentSession, filePath);
        return targetHeader != null;
    }

    public async Task DeleteFileAsync(GridFsFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(_documentSession, filePath);
        if (targetHeader is null)
        {
            return;
        }

        // Delete all file blocks associated with this header.
        await _fileBlockProcedures.DeleteAsync(_documentSession, targetHeader);

        // Delete the header itself.
        await _fileHeaderProcedures.DeleteAsync(_documentSession, targetHeader);
    }

    public async Task<GridFsFileInfo?> GetFileInfoAsync(GridFsFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(_documentSession, filePath);
        if (targetHeader == null)
        {
            return null;
        }

        return new GridFsFileInfo()
        {
            FilePath = filePath,
            StoredLength = targetHeader.StoredLength,
            UserDataLong = targetHeader.UserDataLong,
            UserDataGuid = targetHeader.UserDataGuid,
            Sha256 = targetHeader.Sha256,
            OriginalLength = targetHeader.OriginalLength,
            UpdateDateTime = targetHeader.UpdatedDateTime
        };
    }

    public IDocumentSession DocumentSession => _documentSession;
}