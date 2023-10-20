using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        #region Index
        public IActionResult Index()
        {
            var productsFromDb = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");
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
            
            var productFromDb = _unitOfWork.ProductRepository.Get(p => p.Id == myId, includeProperties:"Category");

            if(productFromDb == null) return NotFound();

            return View(productFromDb);
        }
    }
}
