using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The RenameResourceAsync method is used to rename a resource from one name to another.  This includes moving a resource
    /// between buckets.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="sourceBucketName">The name of the source bucket.</param>
    /// <param name="sourceResourcePath">The name of the resource to be renamed.</param>
    /// <param name="destinationBucketName">The name of the destination bucket.</param>
    /// <param name="destinationResourcePath">The new name for the resource within the <paramref name="destinationBucketName"/></param>
    /// <param name="replaceDestination">Default: false.  A boolean that indicates if an error should be thrown if there is already a resource in the destination location.</param>
    /// <exception cref="BucketNotFoundException">Thrown when either the <paramref name="sourceBucketName"/> or <paramref name="destinationBucketName"/> is not found.</exception>
    /// <exception cref="ResourceNotFoundException">Throw when there isn't a resource at the location specified by <paramref name="sourceBucketName"/> and <paramref name="sourceResourcePath"/>.</exception>
    /// <exception cref="OverwriteNotPermittedException">Throw when there an existing resource at the specified destination and <paramref name="replaceDestination"/> is false.</exception>
    public async Task RenameResourceAsync(IDocumentSession documentSession,
        string sourceBucketName, ContentRepositoryResourcePath sourceResourcePath,
        string destinationBucketName, ContentRepositoryResourcePath destinationResourcePath,
        bool replaceDestination = false)
    {
        // Lookup the bucket
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, sourceBucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(sourceBucketName);
        }

        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, sourceResourcePath);
        if (sourceHeader == null)
        {
            throw new ResourceNotFoundException(sourceBucketName, sourceResourcePath);
        }

        // Lookup the new bucket
        var newBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, destinationBucketName);
        if (newBucket == null)
        {
            throw new BucketNotFoundException(destinationBucketName);
        }

        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, destinationResourcePath);
        if (targetHeader != null)
        {
            if (!replaceDestination)
            {
                throw new OverwriteNotPermittedException(destinationBucketName, destinationResourcePath);
            }

            // Delete the file identified by newFilePath
            await DeleteResourceAsync(documentSession, sourceBucketName, destinationResourcePath);
        }

        sourceHeader.BucketId = newBucket.Id;
        sourceHeader.FilePath = destinationResourcePath;
        sourceHeader.Directory = destinationResourcePath.Directory;
        documentSession.Store(sourceHeader);
    }
}