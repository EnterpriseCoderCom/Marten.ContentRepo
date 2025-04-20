using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

public class ContentResourceBlockProcedures
{
    public Task DeleteAsync(IDocumentSession documentSession, ContentResourceHeader targetHeader)
    {
        documentSession.DeleteWhere<ContentResourceBlock>(x => x.ParentResourceHeaderId == targetHeader.Id);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<ContentResourceBlock> Select(IDocumentSession documentSession, ContentResourceHeader targetHeader)
    {
        var blockResults = documentSession.Query<ContentResourceBlock>()
            .Where(x => x.ParentResourceHeaderId == targetHeader.Id)
            .OrderBy(x => x.BlockSequenceNumber)
            .ToAsyncEnumerable();

        return blockResults;
    }

    public Task UpsertAsync(IDocumentSession documentSession, ContentResourceBlock targetBlock)
    {
        documentSession.Store(targetBlock);
        return Task.CompletedTask;
    }
}