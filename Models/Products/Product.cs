
namespace Profakt.Intranet.Models.Products
{
    public class ProductEntity : BaseEntity
    {
        public string StripeProductId { get; set; } = null!;
        public string StripePriceId { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string FilePath { get; set; } = null!;
        public string FileName { get; set; } = null!;

        public string BuyUrl { get; set; } = null!; //the url to buy the product online

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}
