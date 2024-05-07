using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ConstantDefines.Role_Admin)] //this can also be set individually for any action
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        #region Index
        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _unitOfWork.ApplicationUserRepository.GetAll(includeProperties: "Company");

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in users)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;
            }

            return Json(new { data = users });
        }

        [HttpPost]
        public IActionResult BanUnban([FromBody] string userId)
        {

            var userFromDb = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId, tracked: true);
            if (userFromDb == null)
            {
                return Json(new { success = false, message = "Error while Banning/Unbanning" });
            }

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
            {
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(100);
            }

            _unitOfWork.Save();

            return Json(new { success = true, message = "Modification Successful!" });
        }

        #endregion
    }
}
