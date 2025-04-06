using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

public class ContentBucketProcedures
{
    public async Task<ContentBucket> CreateBucketAsync(IDocumentSession session, string bucketName)
    {
        ContentBucket newBucket = new ContentBucket()
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

    public Task<ContentBucket?> SelectBucketAsync(IDocumentSession session, string bucketName)
    {
        return session.Query<ContentBucket>().SingleOrDefaultAsync(x => x.BucketName == bucketName);
    }
    
    public async Task DeleteBucketAsync(IDocumentSession session, string bucketName)
    {
        ContentBucket? bucket = await SelectBucketAsync(session, bucketName);
        if (bucket != null)
        {
            session.Delete(bucket);
        }
    }
}