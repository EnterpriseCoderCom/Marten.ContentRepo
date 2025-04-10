using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// <para>
    /// <c>CreateBucketAsync</c> creates a new bucket within the system.  A bucket can hold zero to many
    /// content files and serves as a namespace with which to organize content.
    /// </para>
    /// </summary>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the content bucket to be created.</param>
    /// <returns></returns>
    public async Task CreateBucketAsync(IDocumentSession documentSession, string bucketName)
    {
        // Look up the bucket.  Don't create it if it already exists.
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            await _contentBucketProcedures.CreateBucketAsync(documentSession, bucketName);
        }
    }
}