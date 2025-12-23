using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profakt.Intranet.Models.Products
{
    public class DownloadToken : BaseEntity
    {
        // Token der sendes i URL
        public string Token { get; set; } = null!;

        public Guid OrderId { get; set; }

        // Sikkerhed
        public DateTimeOffset ExpiresAt { get; set; }
        public int MaxDownloads { get; set; } = 3;
        public int DownloadCount { get; set; } = 0;

        public bool IsRevoked { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

}
