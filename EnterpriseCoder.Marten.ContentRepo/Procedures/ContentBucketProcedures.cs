using EnterpriseCoder.Marten.ContentRepo.CompiledQueries;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

public class ContentBucketProcedures
{
    public async Task<ContentBucket> CreateBucketAsync(IDocumentSession session, string bucketName)
    {
        var newBucket = new ContentBucket
        {
            // Id is assigned by the constructor.
            BucketName = bucketName
        };

        // Store the newly created bucket.
        session.Store(newBucket);

        // Immediately commit so that future lookups by name succeed.
        // Marten is not able to query items that are not by Id (especially in a LightweightSession)
        // and since Buckets are looked up by name...we need to immediately save the bucket to the database
        // so other code will actually be able to lookup the bucket's ID.
        await session.SaveChangesAsync();

        return newBucket;
    }

    public async Task<ContentBucket?> SelectBucketAsync(IDocumentSession session, string bucketName)
    {
        return await session.QueryAsync(new QuerySelectBucketByName { BucketName = bucketName });
    }

    public async Task<ContentBucket?> SelectBucketByIdAsync(IDocumentSession session, Guid bucketId)
    {
        return await session.LoadAsync<ContentBucket>(bucketId);
    }

    public async Task DeleteBucketAsync(IDocumentSession session, string bucketName)
    {
        var bucket = await SelectBucketAsync(session, bucketName);
        if (bucket != null)
        {
            session.Delete(bucket);
        }
    }
}