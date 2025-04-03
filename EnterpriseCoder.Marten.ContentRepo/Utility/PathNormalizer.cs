using System.Text;

namespace EnterpriseCoder.Marten.ContentRepo.Utility;

public static class PathNormalizer
{
    internal static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Invalid path", nameof(path));
        }

        List<string> buildParts = new List<string>();

        List<string> pathParts = path.Replace('\\', '/').Split('/').ToList();

        foreach (var nextPathPart in pathParts)
        {
            string workPathPart = nextPathPart.Trim();
            if (string.IsNullOrEmpty(workPathPart))
            {
                continue;
            }

            if (workPathPart == ".")
            {
                continue; // nothing to do...just skip it.
            }

            if (workPathPart == "..")
            {
                if (buildParts.Count == 0)
                {
                    throw new ArgumentException("Unable to get parent when already at root directory.");
                }

                // Delete the last directory in the buildParts list.
                buildParts.RemoveAt(buildParts.Count - 1);
                continue;
            }

            buildParts.Add(workPathPart);
        }

        // ================================================================================
        // Assembly the parts into a slash separated string.
        // ================================================================================
        StringBuilder returnPath = new StringBuilder();

        foreach (var nextPathPart in buildParts)
        {
            returnPath.Append("/");
            returnPath.Append(nextPathPart);
        }

        string finalPath = returnPath.ToString();
        if (string.IsNullOrEmpty(finalPath))
        {
            finalPath = "/";
        }

        return finalPath;
    }

    internal static string[] SplitPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Invalid path", nameof(path));
        }
       
        List<string> buildParts = new List<string>();

        List<string> pathParts = path.Replace('\\', '/').Split('/').ToList();

        foreach (var nextPathPart in pathParts)
        {
            string workPathPart = nextPathPart.Trim();
            if (string.IsNullOrEmpty(workPathPart))
            {
                continue;
            }

            if (workPathPart == "." || workPathPart == "..")
            {
                throw new ArgumentException($"{nameof(SplitPath)} does not process relative directory information.");
            }

            buildParts.Add(workPathPart);
        }
        
        return buildParts.ToArray();
    }
}