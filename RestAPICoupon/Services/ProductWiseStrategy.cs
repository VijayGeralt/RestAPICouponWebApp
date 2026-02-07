using System;
using System.Linq;
using Newtonsoft.Json;
using RestAPICoupon.Models;

namespace RestAPICoupon.Services
{
    public class ProductWiseStrategy : ICouponStrategy
    {
        // Applicable if target product exists and min cart total (if any) is met
        public bool IsApplicable(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<ProductWiseDetails>(coupon.DetailsJson);

            var total = cart.Items.Sum(i => i.Price * i.Quantity);
            if (d.MinCartTotal.HasValue && total < d.MinCartTotal.Value)
                return false;

            return cart.Items.Any(i => i.ProductId == d.ProductId);
        }

        // Calculates discount only on the target product
        public decimal CalculateDiscount(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<ProductWiseDetails>(coupon.DetailsJson);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == d.ProductId);
            if (item == null) return 0m;

            var discount = (item.Price * item.Quantity) * (d.DiscountPercent / 100m);
            if (d.MaxDiscount.HasValue)
                discount = Math.Min(discount, d.MaxDiscount.Value);

            return Math.Round(discount, 2);
        }

        // Applies discount only to the target item
        public Cart Apply(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<ProductWiseDetails>(coupon.DetailsJson);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == d.ProductId);
            if (item == null) return cart;

            var discount = CalculateDiscount(coupon, cart);
            item.TotalDiscount += discount;

            return cart;
        }
    }
}