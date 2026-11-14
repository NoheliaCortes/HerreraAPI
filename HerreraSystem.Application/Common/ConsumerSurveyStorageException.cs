namespace HerreraSystem.Application.Common;

public sealed class ConsumerSurveyStorageException : Exception
{
    public ConsumerSurveyStorageException(Exception innerException)
        : base("Consumer survey storage operation failed.", innerException)
    {
    }
}
