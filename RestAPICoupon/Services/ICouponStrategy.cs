using System;
using RestAPICoupon.Models;

namespace RestAPICoupon.Services
{
    public interface ICouponStrategy
    {
        // True if coupon can be applied to this cart
        bool IsApplicable(Coupon coupon, Cart cart);

        // Discount amount this coupon provides
        decimal CalculateDiscount(Coupon coupon, Cart cart);

        // Returns updated cart after applying coupon
        Cart Apply(Coupon coupon, Cart cart);
    }
}
