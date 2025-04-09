using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<IList<ContentRepositoryFileInfo>> GetFileListingByUserDataGuidAsync(
        IDocumentSession documentSession,
        string bucketName, Guid userGuid, int oneBasedPage, int pageSize)
    {
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        var pageList =
            await _fileHeaderProcedures.SelectByUserGuid(documentSession, targetBucket, userGuid, oneBasedPage,
                pageSize);

        List<ContentRepositoryFileInfo> returnListing =
            new List<ContentRepositoryFileInfo>(pageList.Select(x => x.ToContentFileInfoDto()));

        return returnListing;
    }
}