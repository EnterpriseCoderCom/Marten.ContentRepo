namespace EnterpriseCoder.Marten.ContentRepo.Utility;

/// <summary>
/// A <see cref="FileStream"/> that automatically deletes the underlying temporary file when disposed.
/// This class is designed for reading from a temporary file and ensures the file is removed
/// after use, even if an error occurs during file operations.
/// </summary>
public class AutoDeleteReadFileStream : FileStream
{
    private readonly string _tempFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoDeleteReadFileStream"/> class
    /// for the specified temporary file.
    /// </summary>
    /// <param name="tempFilename">The path to the temporary file to be read and deleted upon disposal.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tempFilename"/> is null.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be opened for reading.</exception>
    public AutoDeleteReadFileStream(string tempFilename) : base(tempFilename, FileMode.Open, FileAccess.Read,
        FileShare.None)
    {
        _tempFilePath = tempFilename;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="AutoDeleteReadFileStream"/>
    /// and optionally releases the managed resources. Attempts to delete the temporary file
    /// before disposing the stream.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
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

        base.Dispose(disposing);
    }
}