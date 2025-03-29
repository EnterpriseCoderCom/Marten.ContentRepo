namespace EnterpriseCoder.MartenDb.GridFs;

public class GridFsDirectory : IComparable<GridFsDirectory>
{
    private readonly string _resourcePath;

    public GridFsDirectory(string resourcePath)
    {
        _resourcePath = resourcePath;

        _VerifyPath(resourcePath, false);
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

    #region Implicit Operators

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

    #region Public Methods

    public GridFsDirectory ChildDirectory(string childPath)
    {
        _VerifyPath(childPath, true);

        string workPath = _resourcePath;
        if (workPath.EndsWith('/'))
        {
            workPath = workPath.Substring(0, workPath.Length - 1);
        }

        if (childPath.StartsWith("/") is false)
        {
            childPath = "/" + childPath;
        }

        return workPath + childPath;
    }

    public ContentFilePath ChildFile(string resourcePath)
    {
        _VerifyPath(resourcePath, true);

        string workPath = _resourcePath;
        if (workPath.EndsWith('/'))
        {
            workPath = workPath.Substring(0, workPath.Length - 1);
        }

        return workPath + "/" + resourcePath;
    }

    public int CompareTo(GridFsDirectory? other)
    {
        return string.Compare(_resourcePath, other?._resourcePath, StringComparison.Ordinal);
    }

    #endregion

    #region Private Methods

    private void _VerifyPath(string resourcePath, bool relativePath)
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            throw new ArgumentException($"Invalid resource path (null or empty)");
        }

        // =================================================================================================
        // Paths must contain only A-Za-z0-9, '.'  and space characters with / for separators.
        // =================================================================================================

        // First character must be either a "/" or a "." if this is a relative path.
        bool validStart = relativePath;
        if (resourcePath.StartsWith("/"))
        {
            validStart = true;
        }

        if (validStart is false && resourcePath.StartsWith("."))
        {
            throw new ArgumentException($"Relative path is not allowed in this context: {resourcePath}");
        }

        if (validStart is false)
        {
            throw new ArgumentException($"Improperly formed resource path: {resourcePath}");
        }

        // Loop through and make sure all character are valid.
        foreach (var nextChar in resourcePath)
        {
            bool validCharacter = Char.IsLetterOrDigit(nextChar) || nextChar == ' ' || nextChar == '/' ||
                                  nextChar == '.' || nextChar == '_' || nextChar == '-';

            if (validCharacter is false)
            {
                throw new ArgumentException($"Invalid character in resource path: {resourcePath}");
            }
        }
    }

    #endregion
}