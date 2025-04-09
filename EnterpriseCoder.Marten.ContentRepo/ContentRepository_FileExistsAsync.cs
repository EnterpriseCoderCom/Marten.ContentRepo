using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<bool> FileExistsAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryFilePath filePath)
    {
        // Lookup the target bucket.
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket is null)
        {
            return false;
        }

        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, filePath);
        return targetHeader != null;
    }
}