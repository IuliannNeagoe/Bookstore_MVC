using Bookstore.Models.Models;

namespace Bookstore.Models.ViewModels
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> ListedItems { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public OrderDetail OrderDetail { get; set; }
    }
}
