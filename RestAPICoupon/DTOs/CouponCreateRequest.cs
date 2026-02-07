using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestAPICoupon.Models;

namespace RestAPICoupon.DTOs
{
    public class CouponCreateRequest
    {
        public string Code { get; set; }
        public CouponType Type { get; set; }
        public string DetailsJson { get; set; }
    }
}