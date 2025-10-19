using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;
using DirectoryNotFoundException = EnterpriseCoder.Marten.ContentRepo.Exceptions.DirectoryNotFoundException;

namespace EnterpriseCoder.Marten.ContentRepo.Utility;

/// <summary>
/// Provides business-level validation for directory operations.
/// </summary>
/// <remarks>
/// This validator focuses on business rules and database constraints, NOT path format validation.
/// Path format validation is automatically enforced by ContentRepositoryDirectory constructor,
/// which calls PathNormalizer.NormalizePath() and validates:
/// - Null/empty paths rejected
/// - Paths normalized to /component/subcomponent format
/// - No trailing slashes (normalized away)
/// - Relative path components (..) rejected
/// - Path escaping attempts rejected
///
/// This class validates:
/// - Bucket association and existence
/// - Directory existence (duplicates, not found)
/// - Directory emptiness for deletion
/// - Parent/child directory relationships
/// - Implicit vs explicit directory coexistence policy
/// </remarks>
public static class DirectoryValidator
{
    /// <summary>
    /// Validates that a bucket ID is valid (not empty).
    /// Note: Actual bucket existence should be verified by ContentBucketProcedures.
    /// </summary>
    /// <param name="bucketId">The bucket ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when bucketId is empty.</exception>
    public static void ValidateBucketId(Guid bucketId)
    {
        if (bucketId == Guid.Empty)
        {
            throw new ArgumentException("Bucket ID cannot be empty.", nameof(bucketId));
        }
    }

    /// <summary>
    /// Validates that no explicit directory already exists at the given path.
    /// </summary>
    /// <param name="directoryExists">Whether a directory exists at the path.</param>
    /// <param name="bucketId">The bucket ID.</param>
    /// <param name="directoryPath">The directory path.</param>
    /// <exception cref="DirectoryAlreadyExistsException">Thrown when directory already exists.</exception>
    public static void ValidateDirectoryDoesNotExist(
        bool directoryExists,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (directoryExists)
        {
            throw new DirectoryAlreadyExistsException(bucketId, directoryPath.Path);
        }
    }

    /// <summary>
    /// Validates that an explicit directory exists at the given path.
    /// </summary>
    /// <param name="directoryExists">Whether a directory exists at the path.</param>
    /// <param name="bucketId">The bucket ID.</param>
    /// <param name="directoryPath">The directory path.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when directory does not exist.</exception>
    public static void ValidateDirectoryExists(
        bool directoryExists,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (!directoryExists)
        {
            throw new DirectoryNotFoundException(bucketId, directoryPath.Path);
        }
    }

    /// <summary>
    /// Validates that a directory can be deleted (must be empty).
    /// </summary>
    /// <param name="directory">The directory entity to validate for deletion.</param>
    /// <param name="isEmpty">Whether the directory is empty.</param>
    /// <exception cref="DirectoryNotEmptyException">Thrown when directory contains files or subdirectories.</exception>
    public static void ValidateDirectoryIsEmpty(
        ContentDirectory directory,
        bool isEmpty)
    {
        if (directory == null)
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (!isEmpty)
        {
            throw new DirectoryNotEmptyException(directory.DirectoryPath);
        }
    }

    /// <summary>
    /// Checks if a directory is empty by verifying it has no files or subdirectories.
    /// </summary>
    /// <param name="session">The Marten document session.</param>
    /// <param name="bucketId">The bucket ID containing the directory.</param>
    /// <param name="directoryPath">The directory path to check.</param>
    /// <returns>True if the directory is empty; false otherwise.</returns>
    /// <remarks>
    /// A directory is considered empty when:
    /// - It contains no files (checked via ContentResourceHeader.Directory prefix)
    /// - It contains no subdirectories (checked via ContentDirectory.DirectoryPath prefix)
    ///
    /// Note: This checks the ACTUAL storage state. A directory can become empty if:
    /// - All its files are deleted
    /// - All its subdirectories are deleted
    /// </remarks>
    public static async Task<bool> IsDirectoryEmptyAsync(
        IDocumentSession session,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (directoryPath == null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        var path = directoryPath.Path;

        // Check for files with this directory prefix
        // Files are "in" the directory if their Directory field matches or starts with path/
        var hasFiles = await session.Query<ContentResourceHeader>()
            .AnyAsync(f =>
                f.BucketId == bucketId &&
                (f.Directory == path || f.Directory.StartsWith(path + "/")));

        if (hasFiles)
        {
            return false;
        }

        // Check for subdirectories with this directory prefix
        // Subdirectories are "under" the directory if their path starts with path/
        var hasSubdirectories = await session.Query<ContentDirectory>()
            .AnyAsync(d =>
                d.BucketId == bucketId &&
                d.DirectoryPath != path &&
                d.DirectoryPath.StartsWith(path + "/"));

        return !hasSubdirectories;
    }

    /// <summary>
    /// Gets all child directories (immediate children only).
    /// </summary>
    /// <param name="session">The Marten document session.</param>
    /// <param name="bucketId">The bucket ID containing the directory.</param>
    /// <param name="directoryPath">The directory path to get children for.</param>
    /// <returns>A list of immediate child directories.</returns>
    public static async Task<IReadOnlyList<ContentDirectory>> GetChildDirectoriesAsync(
        IDocumentSession session,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (directoryPath == null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        var path = directoryPath.Path;
        var prefix = path == "/" ? "/" : path + "/";

        // Get all directories that start with the prefix
        var allChildren = await session.Query<ContentDirectory>()
            .Where(d =>
                d.BucketId == bucketId &&
                d.DirectoryPath.StartsWith(prefix) &&
                d.DirectoryPath != path)
            .ToListAsync();

        // Filter to immediate children only (no nested children)
        var immediateChildren = new List<ContentDirectory>();
        foreach (var child in allChildren)
        {
            // For root ("/"), all directories are children
            if (path == "/")
            {
                // Check if it's an immediate child (single level deep)
                var parts = child.DirectoryPath.Split('/');
                if (parts.Length == 2 && parts[0] == "" && !string.IsNullOrEmpty(parts[1]))
                {
                    immediateChildren.Add(child);
                }
            }
            else
            {
                // For non-root, check if path after prefix has no additional slashes
                var remainingPath = child.DirectoryPath.Substring(prefix.Length);
                if (!remainingPath.Contains("/"))
                {
                    immediateChildren.Add(child);
                }
            }
        }

        return immediateChildren;
    }

    /// <summary>
    /// Gets the parent directory path for a given directory.
    /// Uses ContentRepositoryDirectory.Parent property.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <returns>The parent directory path, or null if already at root.</returns>
    /// <remarks>
    /// ContentRepositoryDirectory.Parent handles all the path logic.
    /// This method is a convenience wrapper.
    /// </remarks>
    public static ContentRepositoryDirectory? GetParentDirectory(ContentRepositoryDirectory directoryPath)
    {
        if (directoryPath == null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        if (directoryPath.Path == "/")
        {
            return null; // Already at root
        }

        try
        {
            return directoryPath.Parent;
        }
        catch (InvalidPathException)
        {
            return null; // Parent navigation failed, treat as root
        }
    }
}
