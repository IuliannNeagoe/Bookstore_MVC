using Bookstore.Models.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookstore.Models.ViewModels
{
    public class RoleManagementViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<SelectListItem> CompanyNamesList { get; set; }
        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}
