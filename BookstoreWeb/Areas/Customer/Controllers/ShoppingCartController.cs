using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
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

        [BindProperty]
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
            ShoppingCartViewModel = new ShoppingCartViewModel
            {
                ListedItems = items,
                OrderHeader = new()
            };

            foreach (var item in ShoppingCartViewModel.ListedItems)
            {
                item.Price = MathHelper.GetPriceBasedOnQuantity(item);
                ShoppingCartViewModel.OrderHeader.OrderTotal += item.Price * item.Count;
            }

            return View(ShoppingCartViewModel);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.Get(s => s.Id == cartId);
            if (cartFromDb == null) return NotFound();

            cartFromDb.Count++;
            _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.Get(s => s.Id == cartId);
            if (cartFromDb == null) return NotFound();

            if (cartFromDb.Count <= 1)
            {
                //remove from cart
                _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count--;
                _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.Get(s => s.Id == cartId);
            if (cartFromDb == null) return NotFound();
            _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var items = _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == userId, nameof(Product));
            ShoppingCartViewModel = new ShoppingCartViewModel
            {
                ListedItems = items,
                OrderHeader = new()
            };

            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            ShoppingCartViewModel.OrderHeader.PhoneNumber = ShoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartViewModel.OrderHeader.StreetAddress = ShoppingCartViewModel.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.ApplicationUser.City;
            ShoppingCartViewModel.OrderHeader.State = ShoppingCartViewModel.OrderHeader.ApplicationUser.State;
            ShoppingCartViewModel.OrderHeader.PostalCode = ShoppingCartViewModel.OrderHeader.ApplicationUser.PostalCode;

            foreach (var item in ShoppingCartViewModel.ListedItems)
            {
                item.Price = MathHelper.GetPriceBasedOnQuantity(item);
                ShoppingCartViewModel.OrderHeader.OrderTotal += item.Price * item.Count;
            }

            return View(ShoppingCartViewModel);
        }

        [HttpPost]
        [ActionName(nameof(Summary))]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var items = _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == userId, nameof(Product));

            ShoppingCartViewModel.ListedItems = items;

            ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartViewModel.OrderHeader.ApplicationUserId = userId;

            //this should not be populated, because EF thinks that you want to add a new record (in this case, with the same PK)
            //ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            var applicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            //foreach (var item in ShoppingCartViewModel.ListedItems)
            //{
            //    item.Price = MathHelper.GetPriceBasedOnQuantity(item);
            //    ShoppingCartViewModel.OrderHeader.OrderTotal += item.Price * item.Count;
            //}
            bool isCompanyUser = applicationUser.CompanyId.HasValue;

            if (!isCompanyUser)
            {
                //this is a regular customer account
                ShoppingCartViewModel.OrderHeader.PaymentStatus = PaymentStatus.Pending.ToString();
                ShoppingCartViewModel.OrderHeader.OrderStatus = OrderStatus.Pending.ToString();
            }
            else
            {
                //this is a company user
                ShoppingCartViewModel.OrderHeader.PaymentStatus = PaymentStatus.ApprovedForDelayedPayment.ToString();
                ShoppingCartViewModel.OrderHeader.OrderStatus = OrderStatus.Approved.ToString();
            }

            //save OrderHeader info to db
            _unitOfWork.OrderHeaderRepository.Add(ShoppingCartViewModel.OrderHeader);
            _unitOfWork.Save();

            foreach (var item in ShoppingCartViewModel.ListedItems)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCartViewModel.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };

                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }

            if (isCompanyUser)
            {
                //capture payment
                //future stripe implementation
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartViewModel.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }

    }
}

