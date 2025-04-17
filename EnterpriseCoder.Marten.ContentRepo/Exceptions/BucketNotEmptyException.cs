namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class BucketNotEmptyException : Exception
{
    public BucketNotEmptyException(string bucketName, string resourceName) : base(
        $"{bucketName}: {resourceName} cannot be deleted")
    {
    }
}