using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<Guid> BucketExistsAsync(IDocumentSession documentSession, string bucketName)
    {
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        return targetBucket?.Id ?? Guid.Empty;
    }
}
