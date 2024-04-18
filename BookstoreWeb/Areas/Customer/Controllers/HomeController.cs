using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Utility;
using BookstoreWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : ControllerCustomBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Index
        public IActionResult Index()
        {
            var userId = RetrieveUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                HttpContext.Session.SetInt32(ConstantDefines.Session_Cart, _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == userId).Count());
            }

            var productsFromDb = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");
            return View(productsFromDb);
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details(int? myId)
        {
            if (myId == 0 || myId == null) return NotFound();

            ShoppingCart cart = new()
            {
                Product = _unitOfWork.ProductRepository.Get(p => p.Id == myId, includeProperties: "Category"),
                Count = 1,
                ProductId = myId.Value
            };


            if (cart.Product == null) return NotFound();

            return View(cart);
        }

        [HttpPost]
        [Authorize] //we dont need to specify any role, the user just has to be logged in, no matter the role
        public IActionResult Details(ShoppingCart? cart)
        {
            cart.ApplicationUserId = RetrieveUserId();

            var existingCartFromDb = _unitOfWork.ShoppingCartRepository.Get(c => c.ApplicationUserId == cart.ApplicationUserId && c.ProductId == cart.ProductId);
            if (existingCartFromDb != null)
            {
                //user cart already existing for the product, so increment the count
                existingCartFromDb.Count += cart.Count;
                _unitOfWork.ShoppingCartRepository.Update(existingCartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCartRepository.Add(cart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(ConstantDefines.Session_Cart, _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == cart.ApplicationUserId).Count());
            }

            TempData["success"] = "Cart updated successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}
