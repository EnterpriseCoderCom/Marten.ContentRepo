using System.Linq.Expressions;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten.Linq;

namespace EnterpriseCoder.Marten.ContentRepo.CompiledQueries;

public class QuerySelectBucketByName : ICompiledQuery<ContentBucket, ContentBucket?>
{
    public string BucketName { get; set; } = string.Empty;
    
    public Expression<Func<IMartenQueryable<ContentBucket>, ContentBucket?>> QueryIs()
    {
        return q => q.FirstOrDefault( x => x.BucketName == BucketName);
    }
}