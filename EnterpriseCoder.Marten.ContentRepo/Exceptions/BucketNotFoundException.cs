namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class BucketNotFoundException : Exception
{
    public BucketNotFoundException(string bucketName) : base(bucketName)
    {
    }
}