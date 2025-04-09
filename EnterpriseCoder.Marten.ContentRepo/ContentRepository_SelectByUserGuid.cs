using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<PagedContentRepositoryFileInfo> GetFileListingByUserDataGuidAsync(
        IDocumentSession documentSession,
        string bucketName, Guid userGuid, int oneBasedPage, int pageSize)
    {
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        IPagedList<ContentFileHeader> pageList =
            await _fileHeaderProcedures.SelectByUserGuid(documentSession, targetBucket, userGuid, oneBasedPage,
                pageSize);

        return new PagedContentRepositoryFileInfo(pageList);
    }
}