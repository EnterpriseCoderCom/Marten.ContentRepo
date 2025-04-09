using System.Linq.Expressions;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten.Linq;

namespace EnterpriseCoder.Marten.ContentRepo.CompiledQueries;

public class QuerySelectContentFileHeader : ICompiledQuery<ContentFileHeader, ContentFileHeader?>
{
    public Guid BucketId { get; set; }
    public string FilePath { get; set; } = string.Empty;

    public Expression<Func<IMartenQueryable<ContentFileHeader>, ContentFileHeader?>> QueryIs()
    {
        return q => q.SingleOrDefault(
            x => x.BucketId == BucketId && x.FilePath == FilePath);
    }
}