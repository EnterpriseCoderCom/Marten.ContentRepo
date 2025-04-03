using Marten.Schema;
using UUIDNext;

namespace EnterpriseCoder.Marten.ContentRepo.Entities;

public class ContentFileBlock
{
    private const string OrderedIndexName = "contentfileblock_uidx_contentfileblock_sequence";
    
    [Identity]
    public Guid Id { get; set; } = Uuid.NewDatabaseFriendly(Database.PostgreSql);
   
    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public Guid ParentFileHeaderId { get; set; } = Guid.Empty;
    
    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public int BlockSequenceNumber { get; set; }
    
    public byte[] BlockData { get; set; } = null!;
}