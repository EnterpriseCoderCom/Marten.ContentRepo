using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task CreateBucketAsync(IDocumentSession session, string bucketName)
    {
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(session, bucketName);
        if (targetBucket == null)
        {
            await _contentBucketProcedures.CreateBucketAsync(session, bucketName);
        }
    }
}