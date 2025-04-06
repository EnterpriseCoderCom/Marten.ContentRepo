using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task CreateBucketAsync(IDocumentSession session, string bucketName)
    {
        ContentBucket? targetBucket = await _contentBucketProcedures.SelectBucketAsync(session,bucketName);
        if (targetBucket == null)
        {
            await _contentBucketProcedures.CreateBucketAsync(session, bucketName);
        }
    }
}
