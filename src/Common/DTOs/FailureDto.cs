namespace Common.DTOs;

public class FailureDto(IEnumerable<string> errors, FailureCode failureCode)
{
    public IEnumerable<string> Errors { get; set; } = errors;
    public FailureCode FailureCode { get; set; } = failureCode;

    public FailureDto(string error, FailureCode failureCode) : this([error], failureCode) { }

    public static FailureDto Unauthorized(string error) => new(error, FailureCode.Unauthorized);
    public static FailureDto Forbidden(string error) => new(error, FailureCode.Forbidden);
    public static FailureDto BadRequest(string error) => new(error, FailureCode.BadRequest);
    public static FailureDto NotFound(string error) => new(error, FailureCode.NotFound);
    public static FailureDto InternalServer(string error) => new(error, FailureCode.InternalServer);
    public static FailureDto Conflict(string error) => new(error, FailureCode.Conflict);

    public static FailureDto Unauthorized(IEnumerable<string> errors) => new(errors, FailureCode.Unauthorized);
    public static FailureDto Forbidden(IEnumerable<string> errors) => new(errors, FailureCode.Forbidden);
    public static FailureDto BadRequest(IEnumerable<string> errors) => new(errors, FailureCode.BadRequest);
    public static FailureDto NotFound(IEnumerable<string> errors) => new(errors, FailureCode.NotFound);
    public static FailureDto InternalServer(IEnumerable<string> errors) => new(errors, FailureCode.InternalServer);
    public static FailureDto Conflict(IEnumerable<string> errors) => new(errors, FailureCode.Conflict);
}
