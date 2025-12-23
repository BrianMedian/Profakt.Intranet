using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profakt.Intranet.Models.Products
{
    public class WebhookEvent : BaseEntity
    {
        public string StripeEventId { get; set; } = null!;
        public string EventType { get; set; } = null!;

        public string Payload { get; set; } = null!;

        public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    }

}
