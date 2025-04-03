namespace EnterpriseCoder.Marten.ContentRepo.Utility;

public class AutoDeleteReadFileStream : FileStream
{
    private readonly string _tempFilePath;
    
    public AutoDeleteReadFileStream(string tempFilename) : base(tempFilename, FileMode.Open, FileAccess.Read, FileShare.None)
    {
        _tempFilePath = tempFilename;
    }

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