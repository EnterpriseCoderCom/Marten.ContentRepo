﻿using System.Text;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;

namespace EnterpriseCoder.Marten.ContentRepo.Utility;

/// <summary>
/// Provides utility methods for normalizing and splitting file system paths.
/// </summary>
public static class PathNormalizer
{
    /// <summary>
    /// Normalizes a file system path by converting backslashes to forward slashes,
    /// resolving relative path components ('.' and '..'), and ensuring a consistent
    /// path format starting with a forward slash.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>A normalized path string starting with a forward slash, or "/" if the path is empty after normalization.</returns>
    /// <exception cref="InvalidPathException">Thrown when the path is null, empty, whitespace, or attempts to navigate above the root directory.</exception>
    internal static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidPathException("Null or empty path");
        }

        List<string> buildParts = new List<string>();

        List<string> pathParts = path.Replace('\\', '/').Split('/').ToList();

        foreach (var nextPathPart in pathParts)
        {
            var workPathPart = nextPathPart.Trim();
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
                    throw new InvalidPathException("Unable to get parent when already at root directory.");
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
        var returnPath = new StringBuilder();

        foreach (var nextPathPart in buildParts)
        {
            returnPath.Append("/");
            returnPath.Append(nextPathPart);
        }

        var finalPath = returnPath.ToString();
        if (string.IsNullOrEmpty(finalPath))
        {
            finalPath = "/";
        }

        return finalPath;
    }

    /// <summary>
    /// Splits a file system path into its components, removing empty parts and
    /// rejecting relative path components ('.' and '..').
    /// </summary>
    /// <param name="path">The path to split.</param>
    /// <returns>An array of path components.</returns>
    /// <exception cref="InvalidPathException">Thrown when the path is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidCastException">Thrown when the path contains relative directory components ('.' or '..').</exception>
    internal static string[] SplitPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidPathException("Null or empty path");
        }

        var buildParts = new List<string>();

        var pathParts = path.Replace('\\', '/').Split('/').ToList();

        foreach (var nextPathPart in pathParts)
        {
            var workPathPart = nextPathPart.Trim();
            if (string.IsNullOrEmpty(workPathPart))
            {
                continue;
            }

            if (workPathPart == "." || workPathPart == "..")
            {
                throw new InvalidCastException($"{nameof(SplitPath)} does not support relative directory information.");
            }

            buildParts.Add(workPathPart);
        }

        return buildParts.ToArray();
    }
}