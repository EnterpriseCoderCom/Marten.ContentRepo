using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task RenameFileAsync(IDocumentSession documentSession, 
        string oldBucketName, ContentRepositoryFilePath oldFilePath,
        string newBucketName, ContentRepositoryFilePath newFilePath,
        bool replaceDestination = false)
    {
        // Lookup the bucket
        ContentBucket? targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, oldBucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(oldBucketName);
        }
        
        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, oldFilePath);
        if (sourceHeader == null)
        {
            throw new ResourceNotFoundException(oldBucketName, oldFilePath);
        }

        // Lookup the new bucket
        ContentBucket? newBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, newBucketName);
        if (newBucket == null)
        {
            throw new BucketNotFoundException(newBucketName);
        }
        
        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, newFilePath);
        if (targetHeader != null)
        {
            if (!replaceDestination)
            {
                throw new OverwriteNotPermittedException(newBucketName, newFilePath);
            }

            // Delete the file identified by newFilePath
            await DeleteFileAsync(documentSession, oldBucketName, newFilePath);
        }

        sourceHeader.BucketId = newBucket.Id;
        sourceHeader.FilePath = newFilePath;
        sourceHeader.Directory = newFilePath.Directory;
        documentSession.Store(sourceHeader);
    }
}