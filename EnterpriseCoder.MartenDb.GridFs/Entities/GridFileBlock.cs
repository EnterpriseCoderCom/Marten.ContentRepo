using Marten.Schema;
using UUIDNext;

namespace EnterpriseCoder.MartenDb.GridFs.Entities;

public class GridFileBlock
{
    private const string OrderedIndexName = "gridfileblock_uidx_gridfileblock_sequence";
    
    [Identity]
    public Guid Id { get; set; } = Uuid.NewDatabaseFriendly(Database.PostgreSql);
   
    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public Guid ParentFileHeaderId { get; set; } = Guid.Empty;
    
    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public int BlockSequenceNumber { get; set; }
    
    public byte[] BlockData { get; set; } = null!;
}