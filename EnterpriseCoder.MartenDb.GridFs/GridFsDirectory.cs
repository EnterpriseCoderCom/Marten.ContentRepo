using System.Text;

namespace EnterpriseCoder.MartenDb.GridFs;

public class GridFsDirectory : IComparable<GridFsDirectory>
{
    private readonly string _resourcePath;

    public GridFsDirectory(string resourcePath)
    {
        _resourcePath = PathNormalizer.NormalizePath(resourcePath);
    }

    #region Public Properties

    public string Path => _resourcePath;

    public GridFsDirectory Parent
    {
        get
        {
            if (_resourcePath == "/")
            {
                throw new ArgumentException($"Already at root...unable to get parent.");
            }

            int lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new ArgumentException($"Already at root...unable to get parent.");
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

    public static implicit operator string(GridFsDirectory gridFsPath) => gridFsPath.Path;
    public static implicit operator GridFsDirectory(string resourcePath) => new(resourcePath);

    #endregion

    #region Equality Operators

    public bool Equals(GridFsDirectory other)
    {
        return _resourcePath == other._resourcePath;
    }

    public override bool Equals(object? obj)
    {
        return obj is GridFsDirectory other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _resourcePath.GetHashCode();
    }

    #endregion

    #region IComparable<GridFsDirectory> Members

    public int CompareTo(GridFsDirectory? other)
    {
        return string.Compare(_resourcePath, other?._resourcePath, StringComparison.Ordinal);
    }

    #endregion


    #region Public Methods

    public GridFsDirectory Combine(params string[] childDirectories)
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

        return new GridFsDirectory(PathNormalizer.NormalizePath(returnPath.ToString()));
    }

    #endregion
}