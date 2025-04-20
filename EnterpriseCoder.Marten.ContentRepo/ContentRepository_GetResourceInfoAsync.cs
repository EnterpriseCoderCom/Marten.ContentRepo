using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The GetResourceInfoAsync method returns information about the resource specified in the <paramref name="bucketName"/>
    /// and <paramref name="resourcePath"/> arguments.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.
    /// "/myResourcePath/myImage.png"</param>
    /// <returns>A <see cref="ContentRepositoryResourceInfo"/> that contains information about the given resource.  This method
    /// may return a null reference if the specified bucket and resource are not found.
    /// </returns>
    public async Task<ContentRepositoryResourceInfo?> GetResourceInfoAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryResourcePath resourcePath)
    {
        // Lookup the target bucket
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            return null;
        }

        // Lookup the target resource
        var targetHeader = await _resourceHeaderProcedures.SelectAsync(documentSession, targetBucket, resourcePath);
        return targetHeader?.ToContentFileInfoDto(targetBucket.BucketName);
    }
}