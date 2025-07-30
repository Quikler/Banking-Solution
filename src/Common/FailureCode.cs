namespace Common;

public enum FailureCode
{
    BadRequest = 400,
    Unauthorized = 401,
    //PaymentRequired
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    InternalServer = 500,
}
