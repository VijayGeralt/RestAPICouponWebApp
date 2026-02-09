using System;
using RestAPICoupon.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestAPICoupon.DTOs
{
    public class CouponUpdateRequest
    {
        public string Code { get; set; }
        public CouponType Type { get; set; }
        public string DetailsJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxRedemptions { get; set; }
    }
}