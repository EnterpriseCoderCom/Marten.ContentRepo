using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The DeleteResourceAsync method is used to remove content from the repository.  The resource to be removed is
    /// specified by the <paramref name="documentSession"/> and <paramref name="resourcePath"/> arguments.  If the given
    /// resource is not found, then this method returns without error.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.
    /// "/myResourcePath/myImage.png"</param>
    public async Task DeleteResourceAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryResourcePath resourcePath)
    {
        // Lookup the target bucket.
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            return;
        }

        // Lookup the target resource
        var targetHeader = await _resourceHeaderProcedures.SelectAsync(documentSession, targetBucket, resourcePath);
        if (targetHeader is null)
        {
            return;
        }

        // Delete all file blocks associated with this header.
        await _resourceBlockProcedures.DeleteAsync(documentSession, targetHeader);

        // Delete the header itself.
        await _resourceHeaderProcedures.DeleteAsync(documentSession, targetHeader);
    }
}