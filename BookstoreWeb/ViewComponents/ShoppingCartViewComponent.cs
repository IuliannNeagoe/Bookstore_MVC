using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookstoreWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //Note that this is a syntax and its mandatory to have the same naming conventions, as well as the path of the class

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(ConstantDefines.Session_Cart) == null)
                {
                    HttpContext.Session.SetInt32(ConstantDefines.Session_Cart,
                        _unitOfWork.ShoppingCartRepository.GetAll(s => s.ApplicationUserId == claim.Value).Count());
                }

                return View(HttpContext.Session.GetInt32(ConstantDefines.Session_Cart).Value);
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
