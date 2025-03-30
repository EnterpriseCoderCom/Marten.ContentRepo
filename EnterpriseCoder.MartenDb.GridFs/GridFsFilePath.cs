using EnterpriseCoder.MartenDb.GridFs.Utility;

namespace EnterpriseCoder.MartenDb.GridFs;

public sealed class GridFsFilePath : IComparable<GridFsFilePath>
{
    private readonly string _resourcePath;

    public GridFsFilePath(string resourcePath)
    {
        ArgumentNullException.ThrowIfNull(resourcePath);
        if (resourcePath.Trim() == "/")
        {
            throw new ArgumentException("Invalid path", nameof(resourcePath));
        }

        _resourcePath = PathNormalizer.NormalizePath(resourcePath);
    }

    #region Implicit Operators

    public static implicit operator string(GridFsFilePath gridFsFilePath) => gridFsFilePath.Path;
    public static implicit operator GridFsFilePath(string resourcePath) => new(resourcePath);

    #endregion

    #region Equality Members

    private bool Equals(GridFsFilePath other)
    {
        return _resourcePath == other._resourcePath;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is GridFsFilePath other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _resourcePath.GetHashCode();
    }

    #endregion

    #region IComparable<GridFsFilePath> Members

    public int CompareTo(GridFsFilePath? other)
    {
        return string.Compare(_resourcePath, other?._resourcePath, StringComparison.Ordinal);
    }

    #endregion

    #region Public Properties

    public string Path => _resourcePath;

    public string Filename
    {
        get
        {
            int lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new ArgumentException($"Already at root, unable to get parent.");
            }

            string returnString = _resourcePath.Substring(lastSlashPos + 1);
            return returnString;
        }
    }

    public string FilenameNoExtension
    {
        get
        {
            string fullFilename = Filename;
            int lastDotPos = fullFilename.LastIndexOf('.');
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
            int lastDotPosition = Path.LastIndexOf('.');
            if (lastDotPosition == -1)
            {
                throw new ArgumentException($"Could not find file extension.");
            }

            string returnString = _resourcePath.Substring(lastDotPosition);
            return returnString;
        }
    }

    public GridFsDirectory Directory
    {
        get
        {
            int lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new ArgumentException($"Already at root, unable to get parent.");
            }

            string returnString = _resourcePath.Substring(0, lastSlashPos);
            if (returnString.Length == 0)
            {
                returnString = "/";
            }

            return returnString;
        }
    }

    #endregion
}