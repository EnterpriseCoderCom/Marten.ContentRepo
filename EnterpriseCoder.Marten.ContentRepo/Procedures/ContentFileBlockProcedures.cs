using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

public class ContentFileBlockProcedures
{
    public Task DeleteAsync(IDocumentSession documentSession, ContentFileHeader targetHeader)
    {
        documentSession.DeleteWhere<ContentFileBlock>(x => x.ParentFileHeaderId == targetHeader.Id);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<ContentFileBlock> Select(IDocumentSession documentSession, ContentFileHeader targetHeader)
    {
        var blockResults = documentSession.Query<ContentFileBlock>()
            .Where(x => x.ParentFileHeaderId == targetHeader.Id)
            .OrderBy(x => x.BlockSequenceNumber)
            .ToAsyncEnumerable();

        return blockResults;
    }

    public Task UpsertAsync(IDocumentSession documentSession, ContentFileBlock targetBlock)
    {
        documentSession.Store(targetBlock);
        return Task.CompletedTask;
    }
}