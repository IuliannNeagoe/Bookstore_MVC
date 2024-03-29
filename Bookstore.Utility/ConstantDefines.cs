﻿namespace Bookstore.Utility
{
    public static class ConstantDefines
    {
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Employee = "Employee";

    }

    public enum OrderStatus { Pending, Approved, Processing, Shipped, Cancelled, Refunded }
    public enum PaymentStatus { Pending, Approved, ApprovedForDelayedPayment, Rejected }
}
