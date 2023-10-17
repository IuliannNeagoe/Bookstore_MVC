using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Index
        public IActionResult Index()
        {
            List<Category> categories = _unitOfWork.CategoryRepository.GetAll().ToList();

            return View(categories);
        }
        #endregion

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Gets called once the "Create" button is pressed.
        /// The button is part of a form defined with "post-method", so we need the HttpPost data annotation to match that.
        /// </summary>
        /// <param name="categoryToAdd">Comes directly from the RazorPage because of the form databind</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create(Category categoryToAdd)
        {
            //Add custom validation:
            CheckCustomValidations(categoryToAdd);


            //ModelState will check every data annotation validations for the categoryToAdd param
            //The errors from ModelState validations will be displayed in the UI with the asp-validation-for helper tag
            if (!ModelState.IsValid) return View();

            _unitOfWork.CategoryRepository.Add(categoryToAdd);            //add the new category to the db property
            _unitOfWork.Save();                            //commit the changes to the db
            TempData["success"] = "Category created successfully!";
            //after that, redirect to the Index action(where the data-table is) - it will get reloaded with the new category added
            return RedirectToAction("Index", "Category");
        }
        #endregion

        #region Edit
        /// <summary>
        /// Here the parameter comes from the helper tag asp-route-myId.
        /// The parameter name ha to be exactly the same as the one declared in the tag.
        /// E.g. tag: asp-route-idTest  Edit(int? idTest)
        /// </summary>
        /// <param name="myId"></param>
        /// <returns></returns>
        public IActionResult Edit(int? myId)
        {
            Category? category = _unitOfWork.CategoryRepository.Get(u => u.Id == myId);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {

            if (category == null) return NotFound();

            if (!ModelState.IsValid) return View();

            _unitOfWork.CategoryRepository.Update(category);
            _unitOfWork.Save();
            TempData["success"] = "Category updated successfully!";

            return RedirectToAction("Index", "Category");
        }
        #endregion

        #region Delete
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            Category? categoryFromDb = _unitOfWork.CategoryRepository.Get(u => u.Id == id);

            if(categoryFromDb == null) return NotFound();
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == null) return NotFound();

            Category? categoryFromDb = _unitOfWork.CategoryRepository.Get(u => u.Id == id);
            _unitOfWork.CategoryRepository.Remove(categoryFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully!";

            return RedirectToAction("Index", "Category");
        }
        #endregion

        #region CustomValidations

        /// <summary>
        /// There are 2 types of validation: Client-side and Server-side
        /// ---------------------------------------------------------------------------------------------
        /// Client-side can be done by adding the Scripts - _ValidationScriptsPartial to the razor view.
        /// It will look for all the data annotations which you have for your properties and check them automatically
        /// The server won't get any request until all those validations are fulfilled
        /// ---------------------------------------------------------------------------------------------
        /// Server-side if what we have here - additional validations which have to be checked by the server
        /// </summary>

        private void CheckCustomValidations(Category category)
        {
            if (category.Name.ToLower() == category.DisplayOrder.ToString().ToLower())
            {
                //key has to match with the asp-for field from view side. The error will appear under the Name field in this case.
                ModelState.AddModelError("name", "Name and DisplayOrder cannot be the same");
            }
        }

        #endregion
    }
}
