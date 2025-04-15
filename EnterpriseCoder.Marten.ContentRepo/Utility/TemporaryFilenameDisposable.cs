namespace EnterpriseCoder.Marten.ContentRepo.Utility;

/// <summary>
/// A disposable class that creates a temporary file and provides its path, automatically
/// deleting the file when disposed.
/// </summary>
public class TemporaryFilenameDisposable : IDisposable
{
    private readonly string _tempFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFilenameDisposable"/> class,
    /// creating a temporary file with a unique name.
    /// </summary>
    public TemporaryFilenameDisposable()
    {
        _tempFilePath = Path.GetTempFileName();
    }

    /// <summary>
    /// Gets the full path to the temporary file.
    /// </summary>
    public string FilePath => _tempFilePath;

    /// <summary>
    /// Releases resources by attempting to delete the temporary file.
    /// </summary>
    public void Dispose()
    {
        try
        {
            File.Delete(_tempFilePath);
        }
        catch
        {
            //
        }
    }
}