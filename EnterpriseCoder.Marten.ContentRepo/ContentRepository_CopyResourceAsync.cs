using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The CopyFileAsync method is used to make a copy of an existing resource.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="sourceBucketName">The name of the source bucket.</param>
    /// <param name="sourceResourcePath">The name of the resource to be renamed.</param>
    /// <param name="destinationBucketName">The name of the destination bucket.</param>
    /// <param name="destinationResourcePath">The new name for the resource within the <paramref name="destinationBucketName"/></param>
    /// <param name="autoCreateBucket">Default: true.  Set to true to create the destination bucket automatically.</param>
    /// <param name="overwriteDestination">Default: false.  Set to true to overwrite any existing resource at the specified destination location.</param>
    /// <exception cref="BucketNotFoundException">Thrown when either the <paramref name="sourceBucketName"/> or when <paramref name="destinationBucketName"/> is not found and <paramref name="autoCreateBucket"/> is false.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when there isn't a resource at the location specified by <paramref name="sourceBucketName"/> and <paramref name="sourceResourcePath"/>.</exception>
    /// <exception cref="OverwriteNotPermittedException">Thrown when <paramref name="overwriteDestination"/> is false and there's an existing resource at the given destination location.</exception>
    public async Task CopyResourceAsync(IDocumentSession documentSession,
        string sourceBucketName, ContentRepositoryResourcePath sourceResourcePath,
        string destinationBucketName, ContentRepositoryResourcePath destinationResourcePath,
        bool autoCreateBucket = true, bool overwriteDestination = false)
    {
        // Get the old bucket
        var oldBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, sourceBucketName);
        if (oldBucket is null)
        {
            throw new BucketNotFoundException(sourceBucketName);
        }

        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, oldBucket, sourceResourcePath);
        if (sourceHeader == null)
        {
            throw new ResourceNotFoundException(sourceBucketName, sourceResourcePath);
        }

        // Lookup and possibly create the new bucket.
        var newBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, destinationBucketName);
        if (newBucket is null && autoCreateBucket)
        {
            newBucket = await _contentBucketProcedures.CreateBucketAsync(documentSession, destinationBucketName);
        }

        if (newBucket is null)
        {
            throw new BucketNotFoundException(destinationBucketName);
        }

        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, newBucket, destinationResourcePath);
        if (targetHeader != null)
        {
            if (!overwriteDestination)
            {
                throw new OverwriteNotPermittedException(destinationBucketName, destinationResourcePath);
            }
        }

        // Copy the file
        await using var oldFileStream = await DownloadStreamAsync(documentSession, sourceBucketName, sourceResourcePath);
        if (oldFileStream == null)
        {
            throw new ApplicationException($"Unable to load {nameof(sourceResourcePath)}");
        }

        await UploadStreamAsync(documentSession, destinationBucketName, destinationResourcePath, oldFileStream, autoCreateBucket,
            overwriteDestination, sourceHeader.UserDataGuid, sourceHeader.UserDataLong);
    }
}