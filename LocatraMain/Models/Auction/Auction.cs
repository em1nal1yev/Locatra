﻿using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace LocatraMain.Models.Auction
{
    public enum AuctionStatus
    {
        Pending,
        Active,
        Ended,
        Rejected
    }
    public class Auction
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public decimal StartingPrice { get; set; }

        public decimal? CurrentPrice { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public AuctionStatus Status { get; set; } = AuctionStatus.Pending;

        
        public string CreatedById { get; set; }  
        public ApplicationUser CreatedBy { get; set; } 

        public string? WinnerId { get; set; }
        public ApplicationUser? Winner { get; set; }

        public bool IsPaid { get; set; } = false;
        public ICollection<Bid> Bids { get; set; }
        public ICollection<AuctionImage> Images { get; set; }
    }
}
