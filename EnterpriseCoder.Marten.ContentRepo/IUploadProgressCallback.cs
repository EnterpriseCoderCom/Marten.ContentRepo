namespace EnterpriseCoder.Marten.ContentRepo;

public interface IUploadProgressCallback
{
    Task ReportProgressAsync(int percentComplete);
}