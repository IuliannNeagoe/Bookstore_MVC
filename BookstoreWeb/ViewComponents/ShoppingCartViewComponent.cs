using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookstoreWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        //Note that this is a syntax and its mandatory to have the same naming conventions, as well as the path of the class

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // var user = await _userManager.FindByEmailAsync("test123@gmail.com");

            //string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            //IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(user, resetToken, "Admin123!");


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
