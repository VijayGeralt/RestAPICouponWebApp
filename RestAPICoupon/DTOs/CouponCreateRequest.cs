using RestAPICoupon.Models;
using System;

namespace RestAPICoupon.DTOs
{
    /// <summary>
    /// 
    /// </summary>
    public class CouponCreateRequest
    {
        public string Code { get; set; }
        public CouponType Type { get; set; }
        public string DetailsJson { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxRedemptions { get; set; }
    }
}