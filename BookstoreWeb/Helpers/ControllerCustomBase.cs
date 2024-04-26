using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BookstoreWeb.Helpers
{
    public abstract class ControllerCustomBase : Controller
    {
        public string Domain { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            Domain = Request.Scheme + "://" + Request.Host.Value + "/";
        }

        protected string? RetrieveUserId()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId;
        }
    }
}
