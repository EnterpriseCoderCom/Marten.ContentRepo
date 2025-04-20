using Marten.Schema;
using UUIDNext;

namespace EnterpriseCoder.Marten.ContentRepo.Entities;

public class ContentResourceBlock
{
    private const string OrderedIndexName = "contentresourceblock_uidx_contentresourceblock_sequence";

    [Identity] public Guid Id { get; set; } = Uuid.NewDatabaseFriendly(Database.PostgreSql);

    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public Guid ParentResourceHeaderId { get; set; } = Guid.Empty;

    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public int BlockSequenceNumber { get; set; }

    public byte[] BlockData { get; set; } = null!;
}