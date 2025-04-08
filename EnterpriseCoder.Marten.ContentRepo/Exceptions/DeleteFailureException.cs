namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class DeleteFailureException : Exception
{
    public DeleteFailureException(string bucketName, string resourceName) : base(
        $"{bucketName}: {resourceName} cannot be deleted")
    {
    }
}