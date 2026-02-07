using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestAPICoupon.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Total discount applied to this line item
        public decimal TotalDiscount { get; set; }
    }
}