namespace TravelRequests.Domain.Shared;

public class ResponsePackage<T>
{
    public ResponsePackage() { }

    public ResponsePackage(string message, T result, ErrorResponse? errors)
    {
        Message = message;
        Result = result;
        Errors = errors;
    }

    public string? Message { get; set; }
    public T? Result { get; set; }
    public ErrorResponse? Errors { get; set; }
}
