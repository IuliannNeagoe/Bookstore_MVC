using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
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

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var orderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: nameof(ApplicationUser));

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
