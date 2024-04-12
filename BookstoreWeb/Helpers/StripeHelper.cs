using Stripe;
using Stripe.Checkout;

namespace BookstoreWeb.Helpers
{
    public static class StripeHelper
    {
        public static Session CreateStripeSession(IEnumerable<SessionLineItemOptions> lineItems, string successUrl, string cancelUrl)
        {
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems = lineItems.ToList(),
                Mode = "payment",
            };
            var service = new Stripe.Checkout.SessionService();
            return service.Create(options);
        }

        public static Refund CreateStripeRefund(string paymentIntentId)
        {
            var options = new RefundCreateOptions()
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = paymentIntentId
            };

            var service = new RefundService();
            return service.Create(options);
        }
    }
}
