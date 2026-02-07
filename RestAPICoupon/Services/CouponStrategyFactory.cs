using System;
using RestAPICoupon.Models;

namespace RestAPICoupon.Services
{
    public class CouponStrategyFactory
    {
        // Selects the strategy based on coupon type
        public ICouponStrategy Get(CouponType type)
        {
            switch (type)
            {
                case CouponType.CartWise:
                    return new CartWiseStrategy();
                case CouponType.ProductWise:
                    return new ProductWiseStrategy();
                case CouponType.BxGy:
                    return new BxGyStrategy();
                default:
                    throw new InvalidOperationException("Unknown coupon type.");
            }
        }
    }
}