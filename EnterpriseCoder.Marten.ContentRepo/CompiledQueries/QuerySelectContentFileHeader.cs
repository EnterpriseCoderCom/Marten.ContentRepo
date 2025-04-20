using System.Linq.Expressions;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten.Linq;

namespace EnterpriseCoder.Marten.ContentRepo.CompiledQueries;

public class QuerySelectContentFileHeader : ICompiledQuery<ContentResourceHeader, ContentResourceHeader?>
{
    public Guid BucketId { get; set; }
    public string FilePath { get; set; } = string.Empty;

    public Expression<Func<IMartenQueryable<ContentResourceHeader>, ContentResourceHeader?>> QueryIs()
    {
        return q => q.SingleOrDefault(
            x => x.BucketId == BucketId && x.ResourcePath == FilePath);
    }
}