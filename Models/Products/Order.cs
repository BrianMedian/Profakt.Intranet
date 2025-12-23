using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profakt.Intranet.Models.Products
{
    public class Order : BaseEntity
    {
        // Stripe-identitet
        public string StripeSessionId { get; set; } = null!;
        public string StripePaymentIntentId { get; set; } = null!;

        // Kunde
        public string CustomerEmail { get; set; } = null!;

        // Hvad er købt
        public Guid ProductId { get; set; }

        // Status
        public OrderStatus Status { get; set; } = OrderStatus.Completed;

        public DateTimeOffset PurchasedAt { get; set; } = DateTimeOffset.UtcNow;
    }

}
