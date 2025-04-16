using EnterpriseCoder.Marten.ContentRepo.Procedures;

namespace EnterpriseCoder.Marten.ContentRepo;

/// <summary>
/// Represents a repository implementation for managing content resources and their interactions using an S3-like bucket-based structure.
/// </summary>
/// <remarks>
/// The <c>ContentRepository</c> class provides methods to perform operations such as creating, deleting, and managing buckets.
/// It also facilitates resource-level operations such as uploading, downloading, renaming, copying, and deletion of resources,
/// along with querying metadata and existence of resources.
///
/// This class requires an IDocumentSession instance from Marten in order to perform it's work.  This allows for database
/// connections to be defined outside of this library.
///
/// Transaction control is also delegated to outside of this library. Generally, <c>ContentRepository</c> will not attempt
/// to control transaction scope.   
/// </remarks>
/// <seealso cref="IContentRepository"/>
public partial class ContentRepository : IContentRepository
{
    private const int FileBlockSize = 65535;

    private readonly ContentBucketProcedures _contentBucketProcedures = new();
    private readonly ContentFileBlockProcedures _fileBlockProcedures = new();
    private readonly ContentFileHeaderProcedures _fileHeaderProcedures = new();
}