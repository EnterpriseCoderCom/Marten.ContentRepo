using System.Text;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using EnterpriseCoder.Marten.ContentRepo.Utility;

namespace EnterpriseCoder.Marten.ContentRepo;

/// <summary>
/// Represents a directory in the content repository.
/// </summary>
/// <remarks>
/// Along with providing utilities for normalizing and splitting directory paths, this class provides methods to
///  enforcing proper structure for incoming path information.  This class has implicit conversion operators to and
/// from "string" to allow for ease of use by the caller of IContentRepository method, while still allowing for
/// enforcement of path structure for incoming path information. 
/// </remarks>
public class ContentRepositoryDirectory : IComparable<ContentRepositoryDirectory>
{
    private readonly string _resourcePath;

    /// <summary>
    /// Constructs a new ContentRepositoryDirectory object.  The incoming path is normalized and validated.
    /// </summary>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception>
    public ContentRepositoryDirectory(string resourcePath)
    {
        _resourcePath = PathNormalizer.NormalizePath(resourcePath);
    }

    #region IComparable<ContentRepositoryDirectory> Members

    /// <summary>
    /// Standard IComparable implementation.  This method is used to compare two ContentRepositoryDirectory objects.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(ContentRepositoryDirectory? other)
    {
        return string.Compare(_resourcePath, other?._resourcePath, StringComparison.Ordinal);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Get the underlying resource path as a string.
    /// </summary>
    public string Path => _resourcePath;

    /// <summary>
    /// Used to calculate the "parent" directory name for this ContentRepositoryDirectory.
    /// </summary>
    /// <exception cref="InvalidPathException">Thrown when the Parent property is used, but the path already represents
    /// the root path.</exception>
    public ContentRepositoryDirectory Parent
    {
        get
        {
            if (_resourcePath == "/")
            {
                throw new InvalidPathException("Already at root...unable to get parent.");
            }

            var lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new InvalidPathException("Already at root...unable to get parent.");
            }

            var returnString = _resourcePath.Substring(0, lastSlashPos);

            if (returnString == "")
            {
                return "/";
            }

            return returnString;
        }
    }

    #endregion

    #region Implicit Members

    /// <summary>
    /// Implicit conversion operator to string.
    /// </summary>
    /// <param name="contentRepositoryPath"></param>
    /// <returns>A string that represents the resource directory.</returns>
    public static implicit operator string(ContentRepositoryDirectory contentRepositoryPath)
    {
        return contentRepositoryPath.Path;
    }

    /// <summary>
    /// Defines an implicit conversion operator to create a ContentRepositoryDirectory object from a string.
    /// </summary>
    /// <param name="resourcePath">The string representation of the resource path to be converted into a ContentRepositoryDirectory object.</param>
    /// <returns>A new instance of the ContentRepositoryDirectory initialized with the provided path.</returns>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception>   
    public static implicit operator ContentRepositoryDirectory(string resourcePath)
    {
        return new ContentRepositoryDirectory(resourcePath);
    }

    #endregion

    #region Equality Operators

    public bool Equals(ContentRepositoryDirectory other)
    {
        return _resourcePath == other._resourcePath;
    }

    public override bool Equals(object? obj)
    {
        return obj is ContentRepositoryDirectory other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _resourcePath.GetHashCode();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Like Path.Combine, this method appends incoming directories into a single, properly formatted, path. 
    /// </summary>
    /// <param name="childDirectories"></param>
    /// <returns>Returns a new ContentRepositoryDirectory object that contains the concatenated path.</returns>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception>  
    public ContentRepositoryDirectory Combine(params string[] childDirectories)
    {
        var returnPath = new StringBuilder(_resourcePath);

        foreach (var childDirectory in childDirectories)
        {
            if (returnPath.ToString().EndsWith("/") is false)
            {
                returnPath.Append("/");
            }

            returnPath.Append(childDirectory);
        }

        return new ContentRepositoryDirectory(PathNormalizer.NormalizePath(returnPath.ToString()));
    }

    /// <summary>
    /// Splits the contained path into an array of strings that represents the individual directories.
    /// </summary>
    /// <returns>A string[] of path parts (split on slash (/))</returns>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception> 
    public string[] SplitPath()
    {
        return PathNormalizer.SplitPath(_resourcePath);
    }

    #endregion
}