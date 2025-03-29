using Marten.Schema;
using UUIDNext;

namespace EnterpriseCoder.MartenDb.GridFs.Entities;

public class GridFileHeader
{
    [Identity]
    public Guid Id { get; set; } = Uuid.NewDatabaseFriendly(Database.PostgreSql);

    [DuplicateField] public string FilePath { get; set; } = string.Empty;
    public byte[] Sha256 { get; set; } = Array.Empty<byte>();
    public DateTimeOffset CreationDateTime { get; set; }
    public DateTimeOffset ModifiedDateTime { get; set; }
    public long OriginalLength { get; set; }
    public long StoredLength { get; set; }

    [DuplicateField] public string Directory { get; set; } = string.Empty;
    
    // User custom data guid - useful for referring to an owner or uploader.
    [DuplicateField]
    public Guid UserDataGuid { get; set; } = Guid.Empty;
    
    // User custom data long - useful for download count
    public long UserDataLong { get; set; }
}