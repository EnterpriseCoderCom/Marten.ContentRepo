using EnterpriseCoder.Marten.ContentRepo.CompiledQueries;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

public class ContentResourceHeaderProcedures
{
    public async Task<ContentResourceHeader?> SelectAsync(IDocumentSession documentSession, ContentBucket targetBucket,
        ContentRepositoryResourcePath resourcePath)
    {
        var targetHeader = await documentSession.QueryAsync(new QuerySelectContentFileHeader
        {
            BucketId = targetBucket.Id,
            FilePath = resourcePath
        });

        return targetHeader;
    }

    public async Task<IPagedList<ContentResourceHeader>> SelectByUserGuid(IDocumentSession documentSession,
        ContentBucket targetBucket, Guid userGuid, int pageNumber, int pageSize)
    {
        IPagedList<ContentResourceHeader> pageList = await documentSession.Query<ContentResourceHeader>()
            .Where(x => x.UserDataGuid == userGuid && x.BucketId == targetBucket.Id)
            .ToPagedListAsync(pageNumber, pageSize);

        return pageList;
    }

    public async Task<IPagedList<ContentResourceHeader>> SelectByUserLong(IDocumentSession documentSession,
        ContentBucket targetBucket, long userLong, int pageNumber, int pageSize)
    {
        IPagedList<ContentResourceHeader> pageList = await documentSession.Query<ContentResourceHeader>()
            .Where(x => x.UserDataLong == userLong && x.BucketId == targetBucket.Id)
            .ToPagedListAsync(pageNumber, pageSize);

        return pageList;
    }

    public Task DeleteAsync(IDocumentSession documentSession, ContentResourceHeader targetHeader)
    {
        documentSession.Delete<ContentResourceHeader>(targetHeader.Id);
        return Task.CompletedTask;
    }

    public Task UpsertAsync(IDocumentSession documentSession, ContentResourceHeader targetHeader)
    {
        documentSession.Store(targetHeader);
        return Task.CompletedTask;
    }
}