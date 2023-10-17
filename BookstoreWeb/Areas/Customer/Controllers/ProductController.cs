using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
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

        #region Upsert
        public IActionResult Upsert(int? myId)
        {
            //obtain the Categories to pass onto the dropdown
            var categoriesFromDb = _unitOfWork.CategoryRepository.GetAll();

            IEnumerable<SelectListItem> categoryList = categoriesFromDb.Select(
                c => new SelectListItem()
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });


            ProductViewModel productViewModel = new ProductViewModel();

            //if ID is not present, then we are Creating
            if (myId == null || myId == 0)
            {
                productViewModel.Product = new Product();
            }
            else
            {
                //else we are updating an existing product
                productViewModel.Product = _unitOfWork.ProductRepository.Get(p => p.Id == myId);
            }

            productViewModel.CategoryList = categoryList;

            return View(productViewModel);
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel productViewModel)
        {
            if (!ModelState.IsValid)
            {
                //we need to populate the CategoryList again
                productViewModel.CategoryList = _unitOfWork.CategoryRepository.GetAll()
                    .Select(c => new SelectListItem() 
                    { 
                        Text = c.Name, 
                        Value=c.Id.ToString()
                    });

                return View(productViewModel);
            }

            _unitOfWork.ProductRepository.Add(productViewModel.Product);
            _unitOfWork.Save();
            TempData["success"] = "Product created successfully!";
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
