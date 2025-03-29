namespace EnterpriseCoder.MartenDb.GridFs;

public readonly struct ContentFilePath : IComparable<ContentFilePath>
{
    private readonly string _resourcePath;

    public ContentFilePath(string resourcePath)
    {
        _resourcePath = resourcePath;
        _VerifyPath(resourcePath, false);
    }

    public static implicit operator string(ContentFilePath contentFilePath) => contentFilePath.Path;
    public static implicit operator ContentFilePath(string resourcePath) => new(resourcePath);

    public string Path => _resourcePath;

    public GridFsDirectory Directory
    {
        get
        {
            int lastSlashPos = Path.LastIndexOf('/');
            if (lastSlashPos == -1)
            {
                throw new ArgumentException($"Already at root...unable to get parent.");
            }

            string returnString = _resourcePath.Substring(0, lastSlashPos);
            if (returnString.Length == 0)
            {
                returnString = "/";
            }

            return returnString;
        }
    }
    
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
        
        // No trailing spaces
        if (resourcePath[^1] == ' ')
        {
            throw new ArgumentException($"Resource path has trailing spaces: {resourcePath}");
        }
    }

    public int CompareTo(ContentFilePath other)
    {
        return string.Compare(_resourcePath, other._resourcePath, StringComparison.Ordinal);
    }
}