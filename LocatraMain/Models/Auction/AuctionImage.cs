using System.ComponentModel.DataAnnotations;

namespace LocatraMain.Models.Auction
{
    public class AuctionImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }

        public int AuctionId { get; set; }
        public Auction Auction { get; set; }
    }
}
