using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task UpdateResourceMetaDataAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryResourcePath resourcePath, IReadOnlyDictionary<string, string> updatedMetaData)
    {
        // Lookup the target bucket.
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket is null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        // Read the resource info
        var header = await _resourceHeaderProcedures.SelectAsync(documentSession, targetBucket, resourcePath);
        if (header is null)
        {
            throw new ResourceNotFoundException(bucketName, resourcePath);
        }
        
        header.Metadata = new Dictionary<string,string>(updatedMetaData);
        await _resourceHeaderProcedures.UpsertAsync(documentSession, header);
    }
}