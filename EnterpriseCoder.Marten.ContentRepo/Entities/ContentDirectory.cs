using Marten.Schema;

namespace EnterpriseCoder.Marten.ContentRepo.Entities;

/// <summary>
/// Represents an explicit directory in the content repository.
/// Explicit directories are created by users via the API, as opposed to implicit directories
/// which are derived from file paths.
/// </summary>
public class ContentDirectory
{
    /// <summary>
    /// Primary key - Unique identifier for this directory (Version 7 GUID).
    /// </summary>
    [Identity]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Foreign key reference to the ContentBucket containing this directory.
    /// </summary>
    [DuplicateField]
    public Guid BucketId { get; set; } = Guid.Empty;

    /// <summary>
    /// Normalized full path of the directory (e.g., "/images/backgrounds").
    /// Paths are stored in lowercase with forward slashes and no trailing slash.
    /// Root directory is represented as "/".
    /// </summary>
    [DuplicateField]
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the directory was created (UTC).
    /// </summary>
    [DuplicateField]
    public DateTimeOffset CreatedDateTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp when the directory was last modified (UTC).
    /// </summary>
    [DuplicateField]
    public DateTimeOffset UpdatedDateTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Custom metadata as key-value pairs.
    /// Provides extensibility for additional directory properties without schema changes.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Flag indicating whether this is a system-created directory (true) or user-created (false).
    /// System directories are typically created as part of automatic operations.
    /// </summary>
    [DuplicateField]
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Determines if two ContentDirectory instances are equal based on their identity.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not ContentDirectory other)
        {
            return false;
        }

        return Id == other.Id;
    }

    /// <summary>
    /// Returns the hash code for this directory based on its identity.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
