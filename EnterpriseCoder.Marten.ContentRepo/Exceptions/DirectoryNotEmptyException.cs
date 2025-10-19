namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when attempting to delete a directory that is not empty.
/// </summary>
/// <remarks>
/// This exception is raised when a directory deletion operation fails because the directory
/// contains files or subdirectories. Directories must be empty before they can be deleted.
/// </remarks>
public class DirectoryNotEmptyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryNotEmptyException"/> class.
    /// </summary>
    /// <param name="directoryPath">The path of the directory that is not empty.</param>
    public DirectoryNotEmptyException(string directoryPath)
        : base($"Cannot delete directory '{directoryPath}' because it contains files or subdirectories. Directory must be empty before deletion.")
    {
        DirectoryPath = directoryPath;
    }

    /// <summary>
    /// Gets the path of the directory that is not empty.
    /// </summary>
    public string DirectoryPath { get; }
}
