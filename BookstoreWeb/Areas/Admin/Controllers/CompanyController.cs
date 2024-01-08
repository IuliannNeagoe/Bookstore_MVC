using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.Models;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ConstantDefines.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var companiesFromDb = _unitOfWork.CompanyRepository.GetAll().ToList();
            return View(companiesFromDb);
        }

        public IActionResult Upsert (int myId)
        {
            Company company;

            if(myId == 0)
            {
                //Create
                company = new Company();
            }
            else
            {
                //update
                company = _unitOfWork.CompanyRepository.Get(c => c.Id == myId);

                if(company == null) return NotFound(); 

            }
        
            return View(company);
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (company == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(company);
            }

            if (company.Id == 0)
            {
                _unitOfWork.CompanyRepository.Add(company);
            }
            else
            {
                _unitOfWork.CompanyRepository.Update(company);
            }
            _unitOfWork.Save();

            TempData["success"] = company.Id == 0 ? "Company created successfully!" : "Company updated successfully!";

            return RedirectToAction("Index", "Company");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = _unitOfWork.CompanyRepository.GetAll();

            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null) return Json(new { success = false, message = "Error! Company ID not valid!" });

            Company? companyToDelete = _unitOfWork.CompanyRepository.Get(p => p.Id == id);
            if (companyToDelete == null) return Json(new { success = false, message = "Error! Product not found in database!" });
           
            _unitOfWork.CompanyRepository.Remove(companyToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Company deleted successfully!" });
        }
    }

   
}
