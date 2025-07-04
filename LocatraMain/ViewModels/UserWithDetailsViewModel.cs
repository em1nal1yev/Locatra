using LocatraMain.Models.Auction;
using LocatraMain.Models.Sale;

namespace LocatraMain.ViewModels
{
    public class UserWithDetailsViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public List<Auction> WonAuctions { get; set; }
        public List<Auction> PaidAuctions { get; set; }
        public List<Product> PurchasedProducts { get; set; }
    }
}
