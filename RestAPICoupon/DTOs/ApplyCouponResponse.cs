using System;
using RestAPICoupon.Models;

namespace RestAPICoupon.DTOs
{
    public class ApplyCouponResponse
    {
        public Cart UpdatedCart { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal FinalPrice { get; set; }
    }
}