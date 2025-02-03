namespace sga_back.Exceptions;

public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {

    }

    public ApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
