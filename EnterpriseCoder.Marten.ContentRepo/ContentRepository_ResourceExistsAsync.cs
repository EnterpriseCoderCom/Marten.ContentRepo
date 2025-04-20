using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The ResourceExistsAsync method determines if there is a resource at the given <paramref name="bucketName"/> and
    /// <paramref name="resourcePath"/> location.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.
    /// "/myResourcePath/myImage.png"</param>
    /// <returns>Returns true if the resource was found.  False if it is not present in the database.</returns>
    public async Task<bool> ResourceExistsAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryResourcePath resourcePath)
    {
        // Lookup the target bucket.
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket is null)
        {
            return false;
        }

        // Lookup the target resource
        var targetHeader = await _resourceHeaderProcedures.SelectAsync(documentSession, targetBucket, resourcePath);
        return targetHeader != null;
    }
}