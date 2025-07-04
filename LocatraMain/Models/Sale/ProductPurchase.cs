namespace LocatraMain.Models.Sale
{
    public class ProductPurchase
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime PurchasedAt { get; set; }
        public string DeliveryAddress { get; set; }
    }
}
