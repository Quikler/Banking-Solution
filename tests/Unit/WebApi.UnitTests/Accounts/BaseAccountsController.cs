using BAL.Services.Account;
using Base.UnitTests;
using Microsoft.AspNetCore.Http;
using Moq;
using WebApi.Controllers.V1;

namespace WebApi.UnitTests.Accounts;

public class BaseAccountsController : BaseUnitTests
{
    protected virtual Mock<IAccontManagementService> AccountManagementService { get; }
    protected virtual AccountsController Controller { get; }

    public BaseAccountsController()
    {
        AccountManagementService = new Mock<IAccontManagementService>();
        Controller = new AccountsController(AccountManagementService.Object)
        {
            ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
    }
}
