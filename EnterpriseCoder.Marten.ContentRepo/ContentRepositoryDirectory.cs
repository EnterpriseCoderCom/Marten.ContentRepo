using System.Text;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using EnterpriseCoder.Marten.ContentRepo.Utility;

namespace EnterpriseCoder.Marten.ContentRepo;

public class ContentRepositoryDirectory : IComparable<ContentRepositoryDirectory>
{
    private readonly string _resourcePath;

    public ContentRepositoryDirectory(string resourcePath)
    {
        _resourcePath = PathNormalizer.NormalizePath(resourcePath);
    }

    #region Public Properties

    public string Path => _resourcePath;

    public ContentRepositoryDirectory Parent
    {
        get
        {
            if (_resourcePath == "/")
            {
                throw new InvalidPathException("Already at root...unable to get parent.");
            }

            int lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new InvalidPathException("Already at root...unable to get parent.");
            }

            string returnString = _resourcePath.Substring(0, lastSlashPos);

            if (returnString == "")
            {
                return "/";
            }

            return returnString;
        }
    }

    #endregion

    #region Implicit Members

    public static implicit operator string(ContentRepositoryDirectory contentRepositoryPath) => contentRepositoryPath.Path;
    public static implicit operator ContentRepositoryDirectory(string resourcePath) => new(resourcePath);

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

    #region IComparable<ContentDirectory> Members

    public int CompareTo(ContentRepositoryDirectory? other)
    {
        return string.Compare(_resourcePath, other?._resourcePath, StringComparison.Ordinal);
    }

    #endregion

    #region Public Methods

    public ContentRepositoryDirectory Combine(params string[] childDirectories)
    {
        StringBuilder returnPath = new StringBuilder(_resourcePath);

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

    public string[] SplitPath()
    {
        return PathNormalizer.SplitPath(_resourcePath);
    }
    
    #endregion
}