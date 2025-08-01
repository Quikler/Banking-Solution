namespace Contracts.V1.Responses;

public class SuccessResponse(string message)
{
    public string Message { get; set; } = message;
}
