using Marten.Schema;

namespace EnterpriseCoder.Marten.ContentRepo.Entities;

public class ContentBucket
{
    [Identity] public Guid Id { get; set; } = Guid.CreateVersion7();

    [UniqueIndex] public string BucketName { get; set; } = string.Empty;
}