﻿namespace Bookstore.Utility
{
    public static class ConstantDefines
    {
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Employee = "Employee";

        public const string Session_Cart = "SessionShoppingCart";

    }

    public enum OrderStatus { Pending, Approved, Processing, Shipped, Cancelled }
    public enum PaymentStatus { Pending, Approved, ApprovedForDelayedPayment, Rejected, Refunded, Cancelled }
}
