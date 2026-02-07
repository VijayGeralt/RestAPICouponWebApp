using System;

namespace RestAPICoupon.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public CouponType Type { get; set; }
        public string DetailsJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxRedemptions { get; set; }
        public int CurrentRedemptions { get; set; }
    }
}