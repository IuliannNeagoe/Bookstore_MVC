using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ConstantDefines.Role_Admin)] //this can also be set individually for any action
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Index
        public IActionResult Index()
        {
            var products = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");
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
                productViewModel.Product = _unitOfWork.ProductRepository.Get(p => p.Id == myId, includeProperties: "ProductImages");
            }

            productViewModel.CategoryList = categoryList;

            return View(productViewModel);
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel productViewModel, List<IFormFile?> files)
        {
            if (!ModelState.IsValid)
            {
                //we need to populate the CategoryList again
                productViewModel.CategoryList = _unitOfWork.CategoryRepository.GetAll()
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    });

                return View(productViewModel);
            }

            if (productViewModel.Product.Id == 0)
            {
                _unitOfWork.ProductRepository.Add(productViewModel.Product);
            }

            _unitOfWork.Save();


            if (files != null)
            {

                foreach (IFormFile file in files)
                {
                    UploadImage(file, productViewModel);
                }

                _unitOfWork.ProductRepository.Update(productViewModel.Product);
                _unitOfWork.Save();
            }


            TempData["success"] = "Product created/Updated successfully!";
            return RedirectToAction("Index", "Product");
        }


        #endregion

        #region Delete

        /// <summary>
        /// obsolete
        /// </summary>
        /// <returns></returns>
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null) return NotFound();
        //    Product? productToDelete = _unitOfWork.ProductRepository.Get(p => p.Id == id);

        //    if (productToDelete == null) return NotFound();

        //    return View(productToDelete);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    if (id == null) return NotFound();

        //    Product? productToDelete = _unitOfWork.ProductRepository.Get(p => p.Id == id);
        //    _unitOfWork.ProductRepository.Remove(productToDelete);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully!";
        //    return RedirectToAction("Index");
        //}

        public IActionResult DeleteImage(int imageId)
        {
            var imageFromDb = _unitOfWork.ProductImageRepository.Get(u => u.Id == imageId, includeProperties: "Product");
            if (imageFromDb != null)
            {
                RemoveImage(imageFromDb.ImageUrl);

                _unitOfWork.ProductImageRepository.Remove(imageFromDb);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Upsert), new { myId = imageFromDb.Product.Id });
        }
        #endregion

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");

            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null) return Json(new { success = false, message = "Error! Product ID not valid!" });

            Product? productToDelete = _unitOfWork.ProductRepository.Get(p => p.Id == id);
            if (productToDelete == null) return Json(new { success = false, message = "Error! Product not found in database!" });

            _unitOfWork.ProductRepository.Remove(productToDelete);

            //remove any images of the product as well

            var productImages = _unitOfWork.ProductImageRepository.GetAll(u => u.ProductId == id);
            foreach (var image in productImages)
            {
                _unitOfWork.ProductImageRepository.Remove(image);
                RemoveImage(image.ImageUrl);
            }

            _unitOfWork.Save();

            return Json(new { success = true, message = "Product deleted successfully!" });
        }
        #endregion

        #region Private methods
        private void UploadImage(IFormFile myFile, ProductViewModel productViewModel)
        {
            string individualProductPath = _webHostEnvironment.WebRootPath + @"\images\product-" + productViewModel.Product.Id;
            try
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(myFile.FileName);

                if (!Directory.Exists(individualProductPath))
                    Directory.CreateDirectory(individualProductPath);

                using (var fileStream = new FileStream(Path.Combine(individualProductPath, fileName), FileMode.Create))
                {
                    myFile.CopyTo(fileStream);
                }

                productViewModel.Product.ProductImages ??= new List<ProductImage>();

                ProductImage productImage = new()
                {
                    ImageUrl = @"\images\product-" + productViewModel.Product.Id + @"\" + fileName,
                    ProductId = productViewModel.Product.Id
                };

                productViewModel.Product.ProductImages.Add(productImage);

                //_unitOfWork.ProductImageRepository.Add(productImage);
            }
            catch (Exception)
            {
                return;
            }
        }

        private void RemoveImage(string? imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl?.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
        }

        #endregion
    }
}
