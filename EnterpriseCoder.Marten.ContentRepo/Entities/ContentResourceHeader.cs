﻿using Marten.Schema;
using UUIDNext;

namespace EnterpriseCoder.Marten.ContentRepo.Entities;

public class ContentResourceHeader
{
    [Identity] public Guid Id { get; set; } = Uuid.NewDatabaseFriendly(Database.PostgreSql);

    [DuplicateField] public Guid BucketId { get; set; } = Guid.Empty;
    [DuplicateField] public string ResourcePath { get; set; } = string.Empty;
    public byte[] Sha256 { get; set; } = Array.Empty<byte>();
    public DateTimeOffset UpdatedDateTime { get; set; } = DateTimeOffset.UtcNow;
    public long OriginalLength { get; set; }
    public long StoredLength { get; set; }

    [DuplicateField] public string Directory { get; set; } = string.Empty;

    // User custom data guid - useful for referring to an owner or uploader.
    [DuplicateField] public Guid UserDataGuid { get; set; } = Guid.Empty;

    // User custom data long - useful for download count
    [DuplicateField] public long UserDataLong { get; set; }
}