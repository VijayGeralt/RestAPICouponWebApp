using System;
using System.Linq;
using Newtonsoft.Json;
using RestAPICoupon.Models;

namespace RestAPICoupon.Services
{
    public class BxGyStrategy : ICouponStrategy
    {
        // Applicable if buy requirements are met and get products exist in cart (for pricing)
        public bool IsApplicable(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<BxGyDetails>(coupon.DetailsJson);

            var total = cart.Items.Sum(i => i.Price * i.Quantity);
            if (d.MinCartTotal.HasValue && total < d.MinCartTotal.Value)
                return false;

            var sets = CalculateEligibleSets(cart, d);
            if (sets <= 0) return false;

            // Ensure we have pricing for all get products
            foreach (var gp in d.GetProducts)
            {
                if (!cart.Items.Any(i => i.ProductId == gp.ProductId))
                    return false;
            }

            return true;
        }

        // Calculates discount as the value of free items
        public decimal CalculateDiscount(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<BxGyDetails>(coupon.DetailsJson);
            var sets = CalculateEligibleSets(cart, d);
            if (sets <= 0) return 0m;

            decimal discount = 0m;
            foreach (var gp in d.GetProducts)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == gp.ProductId);
                if (item == null) continue;

                var freeQty = gp.Quantity * sets;
                discount += freeQty * item.Price;
            }

            return Math.Round(discount, 2);
        }

        // Applies free items as added quantity with discount equal to their price
        public Cart Apply(Coupon coupon, Cart cart)
        {
            var d = JsonConvert.DeserializeObject<BxGyDetails>(coupon.DetailsJson);
            var sets = CalculateEligibleSets(cart, d);
            if (sets <= 0) return cart;

            foreach (var gp in d.GetProducts)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == gp.ProductId);
                if (item == null) continue;

                var freeQty = gp.Quantity * sets;

                // Increase quantity to reflect free items
                item.Quantity += freeQty;

                // Discount equals price of free items
                item.TotalDiscount += freeQty * item.Price;
            }

            return cart;
        }

        // Calculates how many times the coupon can be applied
        private int CalculateEligibleSets(Cart cart, BxGyDetails d)
        {
            if (d.BuyProducts == null || d.BuyProducts.Count == 0)
                return 0;

            int sets = int.MaxValue;

            foreach (var bp in d.BuyProducts)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == bp.ProductId);
                if (item == null || item.Quantity < bp.Quantity)
                    return 0;

                var possible = item.Quantity / bp.Quantity;
                sets = Math.Min(sets, possible);
            }

            // If repetition limit <= 0, treat as unlimited
            if (d.RepetitionLimit > 0)
                sets = Math.Min(sets, d.RepetitionLimit);

            return Math.Max(sets, 0);
        }
    }
}