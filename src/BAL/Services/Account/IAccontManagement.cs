using Common;
using Common.DTOs;

namespace BAL.Services.Account;

public interface IAccontManagement
{
    Task<Result<AuthSuccessDto, FailureDto>> SignupAsync(string email, string password);
    Task<Result<AuthSuccessDto, FailureDto>> LoginAsync(string email, string password);
}
