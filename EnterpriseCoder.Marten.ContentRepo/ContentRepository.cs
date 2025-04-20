using EnterpriseCoder.Marten.ContentRepo.Procedures;

namespace EnterpriseCoder.Marten.ContentRepo;

/// <summary>
/// The <c>ContentRepository</c> class implements the <c>IContentRepository</c> interface. This clas is used to managing
/// content repositories, allowing operations such as creating and deleting buckets, uploading and downloading resources,
/// and managing files and metadata.
/// </summary>
/// <remarks>
/// Transaction Control:  All methods in this interface require a Marten <c>IDocumentSession</c> reference.  It is the
/// responsibility of the caller to ensure that the session is properly committed or rolled back with these exceptions:
/// <list type="bullet">
/// <item><description>All bucket creation is committed immediately using a separate session.  This is a design decision
/// that makes it so this library works with any of the available Marten session types.</description></item>
/// <item><description>The DeleteBucketAsync call removes resources for the bucket in a series of secondary session
/// commits, one page at a time.  This ensures that even in the case of a huge bucket, memory allocate is kept reasonable.
/// The deletion of the actual bucket is still a part of the incoming documentSession.</description></item>
/// </list>
/// </remarks>
/// <seealso cref="IContentRepository"/>
public partial class ContentRepository : IContentRepository
{
    private const int FileBlockSize = 65535;

    private readonly ContentBucketProcedures _contentBucketProcedures = new();
    private readonly ContentResourceBlockProcedures _resourceBlockProcedures = new();
    private readonly ContentResourceHeaderProcedures _resourceHeaderProcedures = new();
}