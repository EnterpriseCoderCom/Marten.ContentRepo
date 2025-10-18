using Marten.Schema;

namespace EnterpriseCoder.Marten.ContentRepo.Entities;

public class ContentResourceBlock
{
    private const string OrderedIndexName = "contentresourceblock_uidx_contentresourceblock_sequence";

    [Identity] public Guid Id { get; set; } = Guid.CreateVersion7();

    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public Guid ParentResourceHeaderId { get; set; } = Guid.Empty;

    [UniqueIndex(IndexType = UniqueIndexType.Computed, IndexName = OrderedIndexName)]
    public int BlockSequenceNumber { get; set; }

    public byte[] BlockData { get; set; } = null!;
}