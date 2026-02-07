using System;
using System.Linq;
using Newtonsoft.Json;
using RestAPICoupon.Models;

namespace RestAPICoupon.Services
{
    public class CartWiseStrategy : ICouponStrategy
    {
        // Checks if cart total meets minimum requirement
        public bool IsApplicable(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<CartWiseDetails>(coupon.DetailsJson);
            var total = cart.Items.Sum(i => i.Price * i.Quantity);
            return total >= d.MinCartTotal;
        }

        // Calculates cart-wise discount with optional cap
        public decimal CalculateDiscount(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<CartWiseDetails>(coupon.DetailsJson);
            var total = cart.Items.Sum(i => i.Price * i.Quantity);

            var discount = total * (d.DiscountPercent / 100m);
            if (d.MaxDiscount.HasValue)
                discount = Math.Min(discount, d.MaxDiscount.Value);

            return Math.Round(discount, 2);
        }

        // Applies discount proportionally across all items
        public Cart Apply(Coupon coupon, Cart cart)
        {
            if (cart.Items.Count == 0) return cart;

            var total = cart.Items.Sum(i => i.Price * i.Quantity);
            if (total <= 0) return cart;

            var discount = CalculateDiscount(coupon, cart);
            if (discount <= 0) return cart;

            decimal running = 0m;
            for (int i = 0; i < cart.Items.Count; i++)
            {
                var item = cart.Items[i];
                var itemTotal = item.Price * item.Quantity;

                // Allocate discount proportionally
                decimal itemDiscount;
                if (i == cart.Items.Count - 1)
                {
                    // Last item fixes rounding difference
                    itemDiscount = discount - running;
                }
                else
                {
                    itemDiscount = Math.Round(discount * (itemTotal / total), 2);
                    running += itemDiscount;
                }

                item.TotalDiscount += itemDiscount;
            }

            return cart;
        }
    }
}