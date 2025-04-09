using Marten.Schema;
using UUIDNext;

namespace EnterpriseCoder.Marten.ContentRepo.Entities;

public class ContentBucket
{
    [Identity] public Guid Id { get; set; } = Uuid.NewDatabaseFriendly(Database.PostgreSql);

    [UniqueIndex] public string BucketName { get; set; } = string.Empty;
}