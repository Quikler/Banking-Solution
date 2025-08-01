using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

public class AuthorizeController : ControllerBase
{
    protected Guid UserId => HttpContext.GetUserId() ?? throw new Exception("User is not authenticated.");
}
