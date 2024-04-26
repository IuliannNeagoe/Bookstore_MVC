using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Bookstore.Utility.Helper;
using BookstoreWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Product = Bookstore.Models.Models.Product;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize]
    public class OrderController : ControllerCustomBase
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderViewModel OrderViewModel { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int? orderId)
        {
            OrderViewModel = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == orderId, includeProperties: nameof(ApplicationUser)),
                OrderDetails = _unitOfWork.OrderDetailRepository.GetAll(o => o.OrderHeaderId == orderId, includeProperties: nameof(Product))
            };

            return View(OrderViewModel);
        }

        [HttpPost]
        [Authorize(Roles = ConstantDefines.Role_Admin + "," + ConstantDefines.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == OrderViewModel.OrderHeader.Id);

            if (orderHeaderFromDb == null) return NotFound();

            orderHeaderFromDb.Name = OrderViewModel.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderViewModel.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderViewModel.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderViewModel.OrderHeader.City;
            orderHeaderFromDb.State = OrderViewModel.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderViewModel.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderViewModel.OrderHeader.Carrier))
                orderHeaderFromDb.Carrier = OrderViewModel.OrderHeader.Carrier;

            if (!string.IsNullOrEmpty(OrderViewModel.OrderHeader.TrackingNumber))
                orderHeaderFromDb.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;

            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = ConstantDefines.Role_Admin + "," + ConstantDefines.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderViewModel.OrderHeader.Id, OrderStatus.Processing);
            _unitOfWork.Save();

            TempData["Success"] = "Order Status Updated Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = OrderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = ConstantDefines.Role_Admin + "," + ConstantDefines.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == OrderViewModel.OrderHeader.Id);
            orderHeaderFromDb.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = OrderViewModel.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = OrderStatus.Shipped.ToString();
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if (orderHeaderFromDb.PaymentStatus == PaymentStatus.ApprovedForDelayedPayment.ToString())
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Status Updated Successfully!";

            return RedirectToAction(nameof(Details), new { orderId = OrderViewModel.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = ConstantDefines.Role_Admin + "," + ConstantDefines.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == OrderViewModel.OrderHeader.Id);

            if (orderHeaderFromDb.PaymentStatus == PaymentStatus.Approved.ToString())
            {
                //payment done, so the customer needs a refund
                StripeHelper.CreateStripeRefund(orderHeaderFromDb.PaymentIntentId);
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderFromDb.Id, OrderStatus.Cancelled, PaymentStatus.Refunded);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderFromDb.Id, OrderStatus.Cancelled, PaymentStatus.Cancelled);
            }

            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully!";
            return RedirectToAction(nameof(Details), new { orderId = OrderViewModel.OrderHeader.Id });
        }

        [HttpPost]
        public IActionResult PayForCompanyUsers()
        {
            OrderViewModel.OrderHeader =
                _unitOfWork.OrderHeaderRepository.Get(o => o.Id == OrderViewModel.OrderHeader.Id,
                    nameof(ApplicationUser));
            OrderViewModel.OrderDetails =
                _unitOfWork.OrderDetailRepository.GetAll(o => o.OrderHeaderId == OrderViewModel.OrderHeader.Id, nameof(Product));

            //capture payment with stripe logic
            IEnumerable<SessionLineItemOptions> lineOptions = OrderViewModel.OrderDetails.Select(item =>
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
                StringHelper.BuildUrl(Domain, nameof(Admin), "Order", nameof(PaymentConfirmation), OrderViewModel.OrderHeader.Id.ToString()),
                StringHelper.BuildUrl(Domain, nameof(Admin), "Order", nameof(Details), OrderViewModel.OrderHeader.Id.ToString()));

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(OrderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int id)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == id);

            if (orderHeaderFromDb.PaymentStatus == PaymentStatus.ApprovedForDelayedPayment.ToString())
            {
                //check if payment was successful
                var service = new SessionService();
                var session = service.Get(orderHeaderFromDb.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, Enum.Parse<OrderStatus>(orderHeaderFromDb.OrderStatus), PaymentStatus.Approved);

                    _unitOfWork.Save();
                }
            }
            return View(id);
        }


        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string? status)
        {
            var userId = RetrieveUserId();

            var orderHeaders = (User.IsInRole(ConstantDefines.Role_Admin) ||
                            User.IsInRole(ConstantDefines.Role_Employee))
                ? _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: nameof(ApplicationUser))
                : _unitOfWork.OrderHeaderRepository.GetAll(o => o.ApplicationUserId == userId, includeProperties: nameof(ApplicationUser));

            switch (status)
            {
                case nameof(OrderStatus.Processing):
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == OrderStatus.Processing.ToString());
                    break;
                case nameof(PaymentStatus.ApprovedForDelayedPayment):
                    orderHeaders = orderHeaders.Where(o => o.PaymentStatus == PaymentStatus.ApprovedForDelayedPayment.ToString());
                    break;
                case nameof(OrderStatus.Shipped):
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == OrderStatus.Shipped.ToString());
                    break;
                case nameof(OrderStatus.Approved):
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == OrderStatus.Approved.ToString());
                    break;
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
