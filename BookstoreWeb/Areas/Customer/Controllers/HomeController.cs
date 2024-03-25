using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ShoppingCartViewModel _shoppingCartViewModel;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ShoppingCartViewModel shoppingCartViewModel)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _shoppingCartViewModel = shoppingCartViewModel;
        }

        #region Index
        public IActionResult Index()
        {
            var productsFromDb = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            if (claimsIdentity.IsAuthenticated)
            {
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                var existingCartFromDb = _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == userId);
                _shoppingCartViewModel.ListedItems = existingCartFromDb.ToList();
            }

            return View(productsFromDb);
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = userId;


            var existingCartFromDb = _unitOfWork.ShoppingCartRepository.Get(c => c.ApplicationUserId == userId && c.ProductId == cart.ProductId);
            if (existingCartFromDb != null)
            {
                //user cart already existing for the product, so increment the count
                existingCartFromDb.Count += cart.Count;
                _unitOfWork.ShoppingCartRepository.Update(existingCartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCartRepository.Add(cart);
            }

            TempData["success"] = "Cart updated successfully!";
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
    }
}
