using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

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
            var orderViewModel = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == orderId, includeProperties: nameof(ApplicationUser)),
                OrderDetails = _unitOfWork.OrderDetailRepository.GetAll(o => o.OrderHeaderId == orderId, includeProperties: nameof(Product))
            };

            return View(orderViewModel);
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string? status)
        {
            var orderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: nameof(ApplicationUser));

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
