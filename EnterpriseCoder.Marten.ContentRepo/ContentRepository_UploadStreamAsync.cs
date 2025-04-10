using System.IO.Compression;
using System.Security.Cryptography;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using EnterpriseCoder.Marten.ContentRepo.Utility;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// <para>
    /// The UploadStreamAsync method is used to insert content contained in <paramref name="inStream"/> into the
    /// content repository under the bucket specified in <paramref name="bucketName"/> and the resource path specified
    /// by <paramref name="resourcePath"/>.
    /// </para>
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket in which to place the content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.  "/myResourcePath/myImage.png"</param>
    /// <param name="inStream">A <c>Stream</c> that contains the content to be inserted into the database.</param>
    /// <param name="autoCreateBucket">Whether the bucket specified by <paramref name="bucketName"/> should automatically be created if it is not already present.</param>
    /// <param name="overwriteExisting"><c>true</c> when this call should overwrite any existing resource of the same bucket and file path.</param>
    /// <param name="userGuid">A <c>GUID</c> user specified value.  Defaults to Guid.Empty.  This value is indexed in the database for fast future lookups.  You may want to use this for referencing a user that performed the upload of the content.</param>
    /// <param name="userValue">A <c>long</c> user specified value that can be used to track data related to the content entry.  This value is indexed in the database for fast lookup.  Perhaps a download counter or a primary key value to another document.</param>
    /// <returns></returns>
    /// <exception cref="BucketNotFoundException">If the <paramref name="bucketName"/> is not found and <paramref name="autoCreateBucket"/> is false this exception will be thrown.</exception>
    /// <exception cref="OverwriteNotPermittedException">If the <paramref name="resourcePath"/> is already in the database and <paramref name="overwriteExisting"/> is false, this exception will be thrown./></exception>
    public async Task UploadStreamAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryFilePath resourcePath,
        Stream inStream, bool autoCreateBucket = true, bool overwriteExisting = false, Guid? userGuid = null,
        long userValue = 0L)
    {
        // Make sure the bucket exists
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (autoCreateBucket && targetBucket is null)
        {
            // Create the target bucket
            targetBucket = await _contentBucketProcedures.CreateBucketAsync(documentSession, bucketName);
        }

        // If there's no targetBucket at this point, then we cannot continue.
        if (targetBucket is null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        if (overwriteExisting is false)
        {
            // See if there's an existing item with the given filePath
            if (await FileExistsAsync(documentSession, bucketName, resourcePath))
            {
                // There's an existing item with the same name...throw an IOException.
                throw new OverwriteNotPermittedException(bucketName, resourcePath);
            }
        }

        // No overwrite protection...call delete to make sure 
        // there's nothing taking the incoming filePath resource name.
        await DeleteFileAsync(documentSession, bucketName, resourcePath);

        // Create a temp file stream to save the incoming stream into...
        using var tempFilename = new TemporaryFilenameDisposable();

        // Allocate a transfer buffer that will be used to break the file into
        // blocks for storage.
        var buffer = new byte[FileBlockSize];

        // Variable that will hold the hash for the incoming file AFTER being compressed.
        byte[] sha256Hash;

        // Accumulator to determine the size of the incoming stream.
        var originalFileSize = 0L;

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
        var header = new ContentFileHeader
        {
            // Id is automatically assigned in constructor
            BucketId = targetBucket.Id,
            Directory = resourcePath.Directory,
            FilePath = resourcePath,
            OriginalLength = originalFileSize,
            StoredLength = fileInfo.Length,
            Sha256 = sha256Hash,
            UserDataLong = userValue,
            UserDataGuid = userGuid ?? Guid.Empty
        };

        // Save the header to the database.
        await _fileHeaderProcedures.UpsertAsync(documentSession, header);

        // Reopen the compressed temp file so we can chunk it up into the database.
        using (var tempInStream = new FileStream(tempFilename.FilePath, FileMode.Open))
        {
            // Create a sequence number so that we can order the blocks when 
            // reading in the future.
            var sequenceNumber = 1;

            // Loop and read the compressed temp file block by block.
            int readCount;
            while ((readCount = await tempInStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                // Make a copy of what was read into a new buffer so it can be given 
                // to a ContentFileBlock for storage in the database.
                var saveBuffer = new byte[readCount];
                Buffer.BlockCopy(buffer, 0, saveBuffer, 0, readCount);

                // Create the ContentFileBlock that will be written to the database.
                var nextBlock = new ContentFileBlock
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