using Bookstore.Models.Models;

namespace Bookstore.Utility.Helper
{
    public static class MathHelper
    {
        public static double GetPriceBasedOnQuantity(ShoppingCart item)
        {
            return item.Count switch
            {
                <= 50 => item.Product.Price,
                <= 100 => item.Product.Price50,
                _ => item.Product.Price100
            };
        }
    }
}
