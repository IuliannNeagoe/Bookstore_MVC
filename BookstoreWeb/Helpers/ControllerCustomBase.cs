using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookstoreWeb.Helpers
{
    public abstract class ControllerCustomBase : Controller
    {
        protected string? RetrieveUserId()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId;
        }
    }
}
