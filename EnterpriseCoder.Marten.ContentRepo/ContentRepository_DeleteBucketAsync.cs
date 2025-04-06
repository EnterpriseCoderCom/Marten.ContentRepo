using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task DeleteBucketAsync(IDocumentSession documentSession, string bucketName, bool force = false)
    {
        // Lookup the bucket
        ContentBucket? targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            return;
        }

        // See if there is any content in the bucket
        bool hasContent = await documentSession.Query<ContentFileHeader>().AnyAsync(x => x.BucketId == targetBucket.Id);
        if (hasContent && force == false)
        {
            throw new IOException(
                $"The Bucket {bucketName} is not empty and cannot be deleted.  Or use force = true on the argument list to {nameof(DeleteBucketAsync)}");
        }

        // Destroy all content in the bucket...
        var contentList = documentSession.Query<ContentFileHeader>()
            .Where(x => x.Directory.StartsWith("/")).ToAsyncEnumerable();

        await foreach (var nextContentItem in contentList)
        {
            await DeleteFileAsync(documentSession, bucketName, nextContentItem.FilePath);
        }

        // Delete the bucket entry itself.
        await _contentBucketProcedures.DeleteBucketAsync(documentSession, bucketName);
    }
}