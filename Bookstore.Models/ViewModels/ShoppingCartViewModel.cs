using Bookstore.Models.Models;

namespace Bookstore.Models.ViewModels
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> ListedItems { get; set; }
        public double OrderTotal { get; set; }
    }
}
