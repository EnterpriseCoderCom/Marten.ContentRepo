using System.IO.Compression;
using System.Security.Cryptography;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Utility;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task UploadStreamAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath,
        Stream inStream, bool overwriteExisting = false, Guid? userGuid = null, long userValue = 0L)
    {
        if (overwriteExisting is false)
        {
            // See if there's an existing item with the given filePath
            if (await FileExistsAsync(documentSession, filePath))
            {
                // There's an existing item with the same name...throw an IOException.
                throw new IOException(
                    $"File {filePath} already exists and {nameof(overwriteExisting)} is set to false.");
            }
        }

        // No overwrite protection...call delete to make sure 
        // there's nothing taking the incoming filePath resource name.
        await DeleteFileAsync(documentSession, filePath);

        // Create a temp file stream to save the incoming stream into...
        using TemporaryFilenameDisposable tempFilename = new TemporaryFilenameDisposable();

        // Allocate a transfer buffer that will be used to break the file into
        // blocks for storage.
        byte[] buffer = new byte[FileBlockSize];

        // Variable that will hold the hash for the incoming file AFTER being compressed.
        byte[] sha256Hash;

        // Accumulator to determine the size of the incoming stream.
        long originalFileSize = 0L;

        // Create a SHA256 hasher to accumulate the hash into as we read the input file stream.
        using (var sha256 = SHA256.Create())
        {
            // Open a temporary file using the allocated temporary filename.  The output will 
            // be emitted to this file.
            using (Stream tempStream = File.OpenWrite(tempFilename.FilePath))
            {
                // Create a zip stream that will compress the data coming from the input stream into
                // the tempStream for storage.
                using (var zipStream = new GZipStream(tempStream, CompressionMode.Compress))
                {
                    // Read and compress the incoming stream to a temporary file.
                    int readCount;
                    while ((readCount = await inStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // Accumulate the sha256 hash for the buffer that was just read.
                        sha256.TransformBlock(buffer, 0, readCount, buffer, 0);

                        // Accumulate the size of the original file.
                        originalFileSize += readCount;

                        // Write out the block to the compression stream.
                        await zipStream.WriteAsync(buffer, 0, readCount);
                    }

                    // Close and finish out the gzip stream.
                    zipStream.Close();
                }
            }

            // Finalize the SHA256 buffer.
            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

            // Assign the SHA256 for later storage.
            sha256Hash = sha256.Hash!;
        }

        // Get file information about the temp file.  We need the final data size for 
        // the ContentFileHeader.
        FileInfo fileInfo = new(tempFilename.FilePath);

        // Create a new header document.
        ContentFileHeader header = new ContentFileHeader()
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

        // Save the header to the database.
        await _fileHeaderProcedures.UpsertAsync(documentSession, header);

        // Reopen the compressed temp file so we can chunk it up into the database.
        using (FileStream tempInStream = new FileStream(tempFilename.FilePath, FileMode.Open))
        {
            // Create a sequence number so that we can order the blocks when 
            // reading in the future.
            int sequenceNumber = 1;

            // Loop and read the compressed temp file block by block.
            int readCount;
            while ((readCount = await tempInStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                // Make a copy of what was read into a new buffer so it can be given 
                // to a ContentFileBlock for storage in the database.
                byte[] saveBuffer = new byte[readCount];
                Buffer.BlockCopy(buffer, 0, saveBuffer, 0, readCount);

                // Create the ContentFileBlock that will be written to the database.
                ContentFileBlock nextBlock = new ContentFileBlock()
                {
                    // Id is assigned by constructor
                    ParentFileHeaderId = header.Id,
                    BlockSequenceNumber = sequenceNumber++,
                    BlockData = saveBuffer
                };

                // Do the physical storage.
                await _fileBlockProcedures.UpsertAsync(documentSession, nextBlock);
            }
        }
    }
}