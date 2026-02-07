using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestAPICoupon.Services
{
    // Cart-wise coupon details stored in DetailsJson
    public class CartWiseDetails
    {
        public decimal MinCartTotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal? MaxDiscount { get; set; }
    }

    // Product-wise coupon details stored in DetailsJson
    public class ProductWiseDetails
    {
        public int ProductId { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinCartTotal { get; set; }
    }

    // BxGy coupon details stored in DetailsJson
    public class BxGyDetails
    {
        public List<BxGyProduct> BuyProducts { get; set; } = new List<BxGyProduct>();
        public List<BxGyProduct> GetProducts { get; set; } = new List<BxGyProduct>();
        public int RepetitionLimit { get; set; }
        public decimal? MinCartTotal { get; set; }
    }

    // Product + quantity pair for BxGy rules
    public class BxGyProduct
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}