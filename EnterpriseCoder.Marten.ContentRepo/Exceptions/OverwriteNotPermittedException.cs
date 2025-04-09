namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class OverwriteNotPermittedException : Exception
{
    public OverwriteNotPermittedException(string bucketName, string resourceName) : base(
        $"Unable to overwrite resource {bucketName}: {resourceName}")
    {
    }
}