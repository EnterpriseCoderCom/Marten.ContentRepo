using System.IO.Compression;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using EnterpriseCoder.Marten.ContentRepo.Utility;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<Stream?> DownloadStreamAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryFilePath filePath)
    {
        // Lookup the target bucket 
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        // Select the header for the given resource.
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, filePath);
        if (targetHeader is null)
        {
            // If there's no header, then there's no such resource.
            throw new ResourceNotFoundException(bucketName, filePath);
        }

        // Create a temporary filename to write into as we load the file from the database
        // block by block.
        var tempFilename = Path.GetTempFileName();
        try
        {
            // Open the temp file...
            using (var tempStream = File.OpenWrite(tempFilename))
            {
                // Read the blocks using an IAsyncEnumerable so we aren't reading all block
                // information into memory in one shot.
                var blockResults = _fileBlockProcedures.Select(documentSession, targetHeader);

                // Write out the blocks one at a time into the temporary stream.
                await foreach (var nextBlock in blockResults)
                {
                    await tempStream.WriteAsync(nextBlock.BlockData, 0, nextBlock.BlockData.Length);
                }
            }

            // Wrap the temp filename is an AutoDeleteReadFileStream so that the temp file 
            // will be deleted when the associated stream is disposed.
            FileStream rereadStream = new AutoDeleteReadFileStream(tempFilename);

            // Wrap the rereadStream in a  gzip decompressing stream.
            var unzipStream = new GZipStream(rereadStream, CompressionMode.Decompress);

            return unzipStream;
        }
        catch (Exception)
        {
            // In the event of an exception, try to delete the 
            // temporary file.  It may fail, depending on where within the 
            // "try" statement the exception occurs.
            try
            {
                File.Delete(tempFilename);
            }
            catch
            {
                // ignored
            }

            throw;
        }
    }
}