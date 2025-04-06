using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<ContentRepositoryFileInfo?> GetFileInfoAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryFilePath filePath)
    {
        // Lookup the target bucket
        ContentBucket? targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            return null;
        }

        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, targetBucket, filePath);
        return targetHeader?.ToContentFileInfoDto();
    }
}