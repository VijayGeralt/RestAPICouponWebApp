using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestAPICoupon.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}