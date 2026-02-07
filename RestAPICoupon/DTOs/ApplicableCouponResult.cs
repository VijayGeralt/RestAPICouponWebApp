using System;
using RestAPICoupon.Models;

namespace RestAPICoupon.DTOs
{
    public class ApplicableCouponResult
    {
        public int CouponId { get; set; }
        public CouponType Type { get; set; }
        public decimal Discount { get; set; }
    }
}