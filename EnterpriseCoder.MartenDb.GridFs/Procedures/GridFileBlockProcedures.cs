using EnterpriseCoder.MartenDb.GridFs.Entities;
using Marten;

namespace EnterpriseCoder.MartenDb.GridFs.Procedures;

public class GridFileBlockProcedures
{
    public Task DeleteAsync(IDocumentSession documentSession, GridFileHeader targetHeader)
    {
        documentSession.DeleteWhere<GridFileBlock>(x => x.ParentFileHeaderId == targetHeader.Id);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<GridFileBlock> Select(IDocumentSession documentSession, GridFileHeader targetHeader)
    {
        var blockResults = documentSession.Query<GridFileBlock>()
            .Where(x => x.ParentFileHeaderId == targetHeader.Id)
            .OrderBy(x => x.BlockSequenceNumber)
            .ToAsyncEnumerable();
        
        return blockResults;
    }
}