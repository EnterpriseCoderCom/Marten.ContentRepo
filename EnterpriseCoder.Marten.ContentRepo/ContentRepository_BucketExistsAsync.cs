using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The BucketExistsAsync method returns true if the bucket specified by <paramref name="bucketName"/> exists.
    /// Otherwise, it returns false. 
    /// </summary>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to check the database.</param>
    /// <param name="bucketName">The name of the content bucket.</param>
    /// <returns>Return true if the bucket name specified by <paramref name="bucketName"/> exists in the database.
    /// Otherwise, it returns false.</returns>
    public async Task<Guid> BucketExistsAsync(IDocumentSession documentSession, string bucketName)
    {
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        return targetBucket?.Id ?? Guid.Empty;
    }
}
