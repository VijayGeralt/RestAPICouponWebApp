using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RestAPICoupon.DTOs
{
    public class ApplicableCouponsResponse
    {
        [JsonProperty("applicable_coupons")]
        public List<ApplicableCouponResult> ApplicableCoupons { get; set; }
    }
}