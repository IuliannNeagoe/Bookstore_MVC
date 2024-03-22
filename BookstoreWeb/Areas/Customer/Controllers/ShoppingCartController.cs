using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }

        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var items = _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == userId, nameof(Product));
            ShoppingCartViewModel = new()
            {
                ListedItems = items,
                OrderTotal = items.Sum(i => MathHelper.GetPriceBasedOnQuantity(i) * i.Count)
            };

            return View(ShoppingCartViewModel);
        }


    }
}
