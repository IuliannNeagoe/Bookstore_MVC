using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Index
        public IActionResult Index()
        {
            var products = _unitOfWork.ProductRepository.GetAll();
            return View(products);
        }

        #endregion

        #region Create
        public IActionResult Create()
        {
            //obtain the Categories to pass onto the dropdown

            var categoriesFromDb = _unitOfWork.CategoryRepository.GetAll();

            IEnumerable<SelectListItem> categoryList = categoriesFromDb.Select(
                c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    });

            //make it accessible from the view
            ViewBag.CategoryList = categoryList;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid) return View();

            _unitOfWork.ProductRepository.Add(product);
            _unitOfWork.Save();
            TempData["success"] = "Product created successfully!";
            return RedirectToAction("Index", "Product");
        }
        #endregion

        #region Edit
        public IActionResult Edit(int? myId)
        {
            Product? productToEdit = _unitOfWork.ProductRepository.Get(p => p.Id == myId);
            if (productToEdit == null) return NotFound();
            
            return View(productToEdit);    
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
           
            if (product == null) return NotFound();

            if (!ModelState.IsValid) return View();

            _unitOfWork.ProductRepository.Update(product);
            _unitOfWork.Save();

            TempData["success"] = "Product updated successfully!";
            return RedirectToAction("Index", "Product");
        }

        #endregion

        #region Delete
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            Product? productToDelete = _unitOfWork.ProductRepository.Get(p => p.Id == id);

            if (productToDelete == null) return NotFound();

            return View(productToDelete);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if(id == null) return NotFound();

            Product? productToDelete = _unitOfWork.ProductRepository.Get(p => p.Id == id);
            _unitOfWork.ProductRepository.Remove(productToDelete);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction("Index");
        }

        #endregion
    }
}
