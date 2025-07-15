using Microsoft.AspNetCore.Mvc;

namespace MedicalInventory.API.Controllers;
public abstract class BaseController : ControllerBase
{
    protected int GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ??
                         User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    protected string GetUserRole()
    {
        return User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value ?? "";
    }

}