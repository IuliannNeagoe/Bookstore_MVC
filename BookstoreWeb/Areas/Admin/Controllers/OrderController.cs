using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Product = Bookstore.Models.Models.Product;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize]
    public class OrderController : Controller
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



        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string? status)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

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
