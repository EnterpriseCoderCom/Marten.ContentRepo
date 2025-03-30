namespace EnterpriseCoder.MartenDb.GridFs.Utility;

public class TemporaryFilenameDisposable : IDisposable
{
    private readonly string _tempFilePath;

    public TemporaryFilenameDisposable()
    {
        _tempFilePath = Path.GetTempFileName();
    }

    public string FilePath => _tempFilePath;

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