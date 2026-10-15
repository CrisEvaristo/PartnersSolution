namespace Partners.Application.Exceptions;

public sealed class ApplicationValidationException : Exception
{
    public ApplicationValidationException(string message)
        : base(message)
    {
    }
}