using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profakt.Intranet.Models.Products
{
    public enum OrderStatus
    {
        Pending = 0,
        Completed = 1,
        Refunded = 2,
        Cancelled = 3
    }
}
