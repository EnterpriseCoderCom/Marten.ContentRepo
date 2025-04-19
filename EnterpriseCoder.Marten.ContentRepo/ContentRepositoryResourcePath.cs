using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using EnterpriseCoder.Marten.ContentRepo.Utility;

namespace EnterpriseCoder.Marten.ContentRepo;

/// <summary>
/// Represents a resource path in the content repository.
/// </summary>
/// <remarks>
/// This class provides methods to extract filename, extension, and directory information from a resource path.  It also
/// provides implicit conversion operators to and from "string" to allow for ease of use by the caller of IContentRepository
/// method, while still allowing for enforcement of path structure for incoming path information. 
/// </remarks>
public sealed class ContentRepositoryResourcePath : IComparable<ContentRepositoryResourcePath>
{
    private readonly string _resourcePath;

    /// <summary>
    /// Constructs a new ContentRepositoryResourcePath object.  The incoming path is normalized and validated. 
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception>   
    public ContentRepositoryResourcePath(string resourcePath)
    {
        ArgumentNullException.ThrowIfNull(resourcePath);
        if (resourcePath.Trim() == "/")
        {
            throw new InvalidPathException(resourcePath);
        }

        _resourcePath = PathNormalizer.NormalizePath(resourcePath);
    }

    #region IComparable<ContentFilePath> Members

    public int CompareTo(ContentRepositoryResourcePath? other)
    {
        return string.Compare(_resourcePath, other?._resourcePath, StringComparison.Ordinal);
    }

    #endregion

    #region Implicit Operators

    public static implicit operator string(ContentRepositoryResourcePath contentRepositoryResourcePath)
    {
        return contentRepositoryResourcePath.Path;
    }

    public static implicit operator ContentRepositoryResourcePath(string resourcePath)
    {
        return new ContentRepositoryResourcePath(resourcePath);
    }

    #endregion

    #region Equality Members

    private bool Equals(ContentRepositoryResourcePath other)
    {
        return _resourcePath == other._resourcePath;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is ContentRepositoryResourcePath other && Equals(other));
    }

    public override int GetHashCode()
    {
        return _resourcePath.GetHashCode();
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Returns the underlying resource path as a string. 
    /// </summary>
    public string Path => _resourcePath;

    /// <summary>
    /// Extracts a filename, with extension, from the resource path.  This method will throw an exception if the path
    /// is invalid.
    /// </summary>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception> 
    public string Filename
    {
        get
        {
            var lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new InvalidPathException("Already at root, unable to get parent.");
            }

            var returnString = _resourcePath.Substring(lastSlashPos + 1);
            return returnString;
        }
    }

    /// <summary>
    /// Returns the filename without the extension.  This method will throw an exception if the path is invalid.
    /// </summary>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception>
    public string FilenameNoExtension
    {
        get
        {
            var fullFilename = Filename;
            var lastDotPos = fullFilename.LastIndexOf('.');
            if (lastDotPos != -1)
            {
                return fullFilename.Substring(0, lastDotPos);
            }

            return fullFilename;
        }
    }

    /// <summary>
    /// Extracts the file extension from the resource path (including the leading ".").
    /// This method will throw an exception if the path is invalid.
    /// </summary>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception>
    public string FileExtension
    {
        get
        {
            var lastDotPosition = Path.LastIndexOf('.');
            if (lastDotPosition == -1)
            {
                throw new InvalidPathException("Could not find file extension.");
            }

            var returnString = _resourcePath.Substring(lastDotPosition);
            return returnString;
        }
    }

    /// <summary>
    /// Returns the directory portion of the resource path.  This method will throw an exception if the path is invalid.
    /// </summary>
    /// <exception cref="InvalidPathException">Thrown when the incoming path is null, empty, or contains invalid characters.</exception>
    public ContentRepositoryDirectory Directory
    {
        get
        {
            var lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new InvalidPathException("Already at root, unable to get parent.");
            }

            var returnString = _resourcePath.Substring(0, lastSlashPos);
            if (returnString.Length == 0)
            {
                returnString = "/";
            }

            return returnString;
        }
    }

    #endregion
}