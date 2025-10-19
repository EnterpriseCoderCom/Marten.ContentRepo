using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

/// <summary>
/// Provides stateless, transaction-safe database operations for ContentDirectory entities.
/// All methods accept an IDocumentSession parameter and are designed to work within caller-managed transactions.
/// </summary>
public class ContentDirectoryProcedures
{
    /// <summary>
    /// Finds a directory by bucket and path.
    /// </summary>
    /// <param name="session">The Marten document session for transaction management.</param>
    /// <param name="bucketId">The ID of the bucket containing the directory.</param>
    /// <param name="directoryPath">The normalized directory path to search for.</param>
    /// <returns>The ContentDirectory if found; null otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when session or directoryPath is null.</exception>
    public async Task<ContentDirectory?> SelectDirectoryByPathAsync(
        IDocumentSession session,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (directoryPath == null)
            throw new ArgumentNullException(nameof(directoryPath));

        var directory = await session.Query<ContentDirectory>()
            .FirstOrDefaultAsync(d =>
                d.BucketId == bucketId &&
                d.DirectoryPath == directoryPath.Path);

        return directory;
    }

    /// <summary>
    /// Gets all directories in a bucket.
    /// </summary>
    /// <param name="session">The Marten document session for transaction management.</param>
    /// <param name="bucketId">The ID of the bucket to query.</param>
    /// <returns>A list of all ContentDirectory entities in the bucket.</returns>
    /// <exception cref="ArgumentNullException">Thrown when session is null.</exception>
    public async Task<IReadOnlyList<ContentDirectory>> SelectDirectoriesByBucketAsync(
        IDocumentSession session,
        Guid bucketId)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        var directories = await session.Query<ContentDirectory>()
            .Where(d => d.BucketId == bucketId)
            .ToListAsync();

        return directories;
    }

    /// <summary>
    /// Gets all directories in a bucket that match a prefix path.
    /// </summary>
    /// <param name="session">The Marten document session for transaction management.</param>
    /// <param name="bucketId">The ID of the bucket to query.</param>
    /// <param name="prefix">The directory prefix to search for.</param>
    /// <returns>A list of ContentDirectory entities matching the prefix.</returns>
    /// <exception cref="ArgumentNullException">Thrown when session or prefix is null.</exception>
    public async Task<IReadOnlyList<ContentDirectory>> SelectDirectoriesByBucketAndPrefixAsync(
        IDocumentSession session,
        Guid bucketId,
        ContentRepositoryDirectory prefix)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (prefix == null)
            throw new ArgumentNullException(nameof(prefix));

        var directories = await session.Query<ContentDirectory>()
            .Where(d =>
                d.BucketId == bucketId &&
                d.DirectoryPath.StartsWith(prefix.Path))
            .ToListAsync();

        return directories;
    }

    /// <summary>
    /// Creates a new directory entity and stores it in the database.
    /// </summary>
    /// <param name="session">The Marten document session for transaction management.</param>
    /// <param name="bucketId">The ID of the bucket where the directory will be created.</param>
    /// <param name="directoryPath">The normalized directory path.</param>
    /// <returns>The created ContentDirectory entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when session or directoryPath is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a directory already exists at the specified path.</exception>
    public async Task<ContentDirectory> CreateDirectoryAsync(
        IDocumentSession session,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (directoryPath == null)
            throw new ArgumentNullException(nameof(directoryPath));

        // Check if directory already exists
        var existing = await SelectDirectoryByPathAsync(session, bucketId, directoryPath);
        if (existing != null)
        {
            throw new InvalidOperationException(
                $"Directory already exists at path '{directoryPath.Path}' in bucket '{bucketId}'.");
        }

        var newDirectory = new ContentDirectory
        {
            BucketId = bucketId,
            DirectoryPath = directoryPath.Path,
            CreatedDateTime = DateTimeOffset.UtcNow,
            UpdatedDateTime = DateTimeOffset.UtcNow,
            IsSystem = false
        };

        session.Store(newDirectory);

        return newDirectory;
    }

    /// <summary>
    /// Deletes a directory entity from the database.
    /// </summary>
    /// <param name="session">The Marten document session for transaction management.</param>
    /// <param name="bucketId">The ID of the bucket containing the directory.</param>
    /// <param name="directoryPath">The normalized directory path to delete.</param>
    /// <returns>True if the directory was deleted; false if it was not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when session or directoryPath is null.</exception>
    public async Task<bool> DeleteDirectoryAsync(
        IDocumentSession session,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (directoryPath == null)
            throw new ArgumentNullException(nameof(directoryPath));

        var directory = await SelectDirectoryByPathAsync(session, bucketId, directoryPath);
        if (directory == null)
        {
            return false;
        }

        session.Delete(directory);
        return true;
    }

    /// <summary>
    /// Checks if a directory exists in the specified bucket.
    /// </summary>
    /// <param name="session">The Marten document session for transaction management.</param>
    /// <param name="bucketId">The ID of the bucket to query.</param>
    /// <param name="directoryPath">The normalized directory path to check.</param>
    /// <returns>True if the directory exists; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when session or directoryPath is null.</exception>
    public async Task<bool> DirectoryExistsAsync(
        IDocumentSession session,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        if (directoryPath == null)
            throw new ArgumentNullException(nameof(directoryPath));

        var exists = await session.Query<ContentDirectory>()
            .AnyAsync(d =>
                d.BucketId == bucketId &&
                d.DirectoryPath == directoryPath.Path);

        return exists;
    }
}
