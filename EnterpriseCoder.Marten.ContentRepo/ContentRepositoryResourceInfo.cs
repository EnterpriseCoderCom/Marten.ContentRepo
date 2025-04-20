namespace EnterpriseCoder.Marten.ContentRepo;

/// <summary>
/// The ContentRepositoryResourceInfo class is used to return information about a resource in the content repository.
/// </summary>
public class ContentRepositoryResourceInfo
{
    /// <summary>
    /// The name of the bucket that contains the resource.
    /// </summary>
    public string BucketName { get; set; } = string.Empty;
    /// <summary>
    /// The ResourcePath location for this resource.
    /// </summary>
    public ContentRepositoryResourcePath ResourcePath { get; set; } = null!;
    /// <summary>
    /// The Sha256 hash of the resource.
    /// </summary>
    public byte[] Sha256 { get; set; } = Array.Empty<byte>();
    /// <summary>
    /// The date and time that this resource was inserted into the repository.
    /// </summary>
    public DateTimeOffset UpdateDateTime { get; set; }
    /// <summary>
    /// The original, uncompressed length of the resource.
    /// </summary>
    public long OriginalLength { get; set; }
    /// <summary>
    /// The compressed, stored length of the resource.
    /// </summary>
    public long StoredLength { get; set; }
    /// <summary>
    /// User-defined data guid.  This can be used to track quota or other user-defined data.  This field
    /// is indexed in the repository for fast lookup. 
    /// </summary>
    public Guid UserDataGuid { get; set; } = Guid.Empty;
    
    /// <summary>
    /// User-defined data long.  This can be used to track quota or other user-defined data.  This field
    /// is indexed in the repository for fast lookup.
    /// </summary>
    public long UserDataLong { get; set; }
}