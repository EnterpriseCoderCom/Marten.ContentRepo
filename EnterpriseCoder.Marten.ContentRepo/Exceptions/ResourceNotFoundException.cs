namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string bucketName, string resourcePath) : base(
        $"{bucketName}: {resourcePath} was not found")
    {
    }
}