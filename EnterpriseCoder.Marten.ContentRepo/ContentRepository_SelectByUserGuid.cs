using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<IList<ContentRepositoryFileInfo>> GetFileListingByUserDataGuidAsync(IDocumentSession documentSession,
        string bucketName, Guid userGuid, int oneBasedPage, int pageSize)
    {
        ContentBucket? targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new IOException($"Bucket {bucketName} not found");
        }

        IPagedList<ContentFileHeader> pageList =
            await _fileHeaderProcedures.SelectByUserGuid(documentSession, targetBucket, userGuid, oneBasedPage,
                pageSize);

        List<ContentRepositoryFileInfo> returnListing =
            new List<ContentRepositoryFileInfo>(pageList.Select(x => x.ToContentFileInfoDto()));

        return returnListing;
    }
}