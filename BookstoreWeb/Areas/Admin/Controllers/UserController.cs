using Bookstore.DataAccess.Data;
using Bookstore.DataAccess.Repositories.Interfaces;
using Bookstore.Models.ViewModels;
using Bookstore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ConstantDefines.Role_Admin)] //this can also be set individually for any action
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public RoleManagementViewModel RoleManagementVM { get; set; }

        public UserController(IUnitOfWork unitOfWork,
            ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
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

        public IActionResult RoleManagement(string userId)
        {
            var userFromDb = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId, includeProperties: "Company");
            var userRoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

            RoleManagementVM = new RoleManagementViewModel()
            {
                ApplicationUser = userFromDb,
                CompanyNamesList = _unitOfWork.CompanyRepository.GetAll().Select(u => new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = u.Id == userFromDb.Company?.Id }),
                RolesList = _roleManager.Roles.Select(u => new SelectListItem() { Text = u.Name, Value = u.Id.ToString(), Selected = u.Name == _roleManager.Roles.FirstOrDefault(r => r.Id == userRoleId).Name })
            };


            return View(RoleManagementVM);
        }

        [HttpPost]
        [ActionName(nameof(RoleManagement))]
        public async Task<IActionResult> RoleManagementPOST()
        {
            var userFromDb = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == RoleManagementVM.ApplicationUser.Id, tracked: true);

            var role = _roleManager.Roles.FirstOrDefault(r => r.Id == RoleManagementVM.ApplicationUser.Role).Name;

            userFromDb.CompanyId = role == ConstantDefines.Role_Company ? RoleManagementVM.ApplicationUser.CompanyId : null;

            var user = await _userManager.FindByIdAsync(userFromDb.Id);
            var userOldRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, userOldRoles);

            await _userManager.AddToRoleAsync(user, role);

            _unitOfWork.Save();
            return RedirectToAction(nameof(RoleManagement), new { userId = RoleManagementVM.ApplicationUser.Id });
        }

        #endregion
    }
}
