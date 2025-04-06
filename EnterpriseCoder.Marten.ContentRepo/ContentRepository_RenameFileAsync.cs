using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task RenameFileAsync(IDocumentSession documentSession, 
        string bucketName, ContentRepositoryFilePath oldFilePath,
        ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false)
    {
        // Lookup the bucket
        ContentBucket? targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new FileNotFoundException($"Bucket {bucketName} not found");
        }
        
        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, oldFilePath);
        if (sourceHeader == null)
        {
            throw new FileNotFoundException(oldFilePath);
        }

        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, newFilePath);
        if (targetHeader != null)
        {
            if (!overwriteDestination)
            {
                throw new IOException($"File {newFilePath} exists and {nameof(overwriteDestination)} is set to false.");
            }

            // Delete the file identified by newFilePath
            await DeleteFileAsync(documentSession, bucketName, newFilePath);
        }

        sourceHeader.FilePath = newFilePath;
        sourceHeader.Directory = newFilePath.Directory;
        documentSession.Store(sourceHeader);
    }
}