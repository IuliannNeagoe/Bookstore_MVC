using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Bookstore.Utility.Helper;
using BookstoreWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : ControllerCustomBase
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

            var userId = RetrieveUserId();
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
                item.Product.ProductImages =
                    _unitOfWork.ProductImageRepository.GetAll(u => u.ProductId == item.ProductId).ToList();
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
            var cartFromDb = _unitOfWork.ShoppingCartRepository.Get(s => s.Id == cartId, tracked: true);
            if (cartFromDb == null) return NotFound();

            if (cartFromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(ConstantDefines.Session_Cart,
                    _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

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
            //tracked has to be true, because we are retrieving the cart once here, then once for the Session, and EF throws a conflict ( I think :/ )
            var cartFromDb = _unitOfWork.ShoppingCartRepository.Get(s => s.Id == cartId, tracked: true);
            if (cartFromDb == null) return NotFound();

            HttpContext.Session.SetInt32(ConstantDefines.Session_Cart,
                _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var userId = RetrieveUserId();
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
            var userId = RetrieveUserId();
            var items = _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == userId, nameof(Product));

            ShoppingCartViewModel.ListedItems = items;

            ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartViewModel.OrderHeader.ApplicationUserId = userId;

            //this should not be populated, because EF thinks that you want to add a new record (in this case, with the same PK)
            //ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            var applicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            foreach (var item in ShoppingCartViewModel.ListedItems)
            {
                item.Price = MathHelper.GetPriceBasedOnQuantity(item);
            }

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
                return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartViewModel.OrderHeader.Id });
            }

            //regular user 
            //capture payment with stripe logic
            IEnumerable<SessionLineItemOptions> lineOptions = ShoppingCartViewModel.ListedItems.Select(item =>
                new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Description = item.Product.Description,
                            Name = item.Product.Title
                        },
                        UnitAmount = (long)item.Price * 100
                    },
                    Quantity = item.Count
                });


            Session session = StripeHelper.CreateStripeSession(lineOptions,
                StringHelper.BuildUrl(Domain, "Customer", "ShoppingCart", nameof(OrderConfirmation), ShoppingCartViewModel.OrderHeader.Id.ToString()),
                StringHelper.BuildUrl(Domain, "Customer", "ShoppingCart", nameof(Index)));

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(ShoppingCartViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == id, nameof(ApplicationUser));
            if (orderHeaderFromDb == null) return NoContent();

            if (orderHeaderFromDb.PaymentStatus != PaymentStatus.ApprovedForDelayedPayment.ToString())
            {
                //order placed by regular user => check if payment was successful
                var service = new SessionService();
                var session = service.Get(orderHeaderFromDb.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, OrderStatus.Approved, PaymentStatus.Approved);

                    //remove shoppingcart from db, because it was purchased
                    var shoppingCarts = _unitOfWork.ShoppingCartRepository
                        .GetAll(s => s.ApplicationUserId == orderHeaderFromDb.ApplicationUserId).ToList();
                    _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);

                    _unitOfWork.Save();

                    HttpContext.Session.Clear();
                }
            }
            else
            {
                //remove shoppingcart from db, because it was purchased
                var shoppingCarts = _unitOfWork.ShoppingCartRepository
                    .GetAll(s => s.ApplicationUserId == orderHeaderFromDb.ApplicationUserId).ToList();
                _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);

                _unitOfWork.Save();

                HttpContext.Session.Clear();
            }



            return View(id);
        }

    }
}

