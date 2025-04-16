using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using EnterpriseCoder.Marten.ContentRepo.Utility;

namespace EnterpriseCoder.Marten.ContentRepo;

public sealed class ContentRepositoryResourcePath : IComparable<ContentRepositoryResourcePath>
{
    private readonly string _resourcePath;

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

    public string Path => _resourcePath;

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