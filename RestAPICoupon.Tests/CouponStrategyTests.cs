using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestAPICoupon.Models;
using RestAPICoupon.Services;
using System.Collections.Generic;

namespace RestAPICoupon.Tests
{
    [TestClass]
    public class CouponStrategyTests
    {
        [TestMethod]
        public void CartWise_Discount_IsCalculated()
        {
            var coupon = new Coupon
            {
                Type = CouponType.CartWise,
                DetailsJson = JsonConvert.SerializeObject(new CartWiseDetails
                {
                    MinCartTotal = 100,
                    DiscountPercent = 10
                })
            };

            var cart = new Cart
            {
                Items = new List<CartItem>
                {
                    new CartItem { ProductId = 1, Price = 50, Quantity = 2 },
                    new CartItem { ProductId = 2, Price = 30, Quantity = 1 }
                }
            };

            var strategy = new CartWiseStrategy();

            Assert.IsTrue(strategy.IsApplicable(coupon, cart));
            var discount = strategy.CalculateDiscount(coupon, cart);
            Assert.AreEqual(13m, discount);
        }

        [TestMethod]
        public void ProductWise_Discount_AppliesOnlyToTarget()
        {
            var coupon = new Coupon
            {
                Type = CouponType.ProductWise,
                DetailsJson = JsonConvert.SerializeObject(new ProductWiseDetails
                {
                    ProductId = 1,
                    DiscountPercent = 20
                })
            };

            var cart = new Cart
            {
                Items = new List<CartItem>
                {
                    new CartItem { ProductId = 1, Price = 100, Quantity = 1 },
                    new CartItem { ProductId = 2, Price = 50, Quantity = 1 }
                }
            };

            var strategy = new ProductWiseStrategy();
            var discount = strategy.CalculateDiscount(coupon, cart);

            Assert.AreEqual(20m, discount);
        }

        [TestMethod]
        public void BxGy_Discount_UsesFreeItems()
        {
            var coupon = new Coupon
            {
                Type = CouponType.BxGy,
                DetailsJson = JsonConvert.SerializeObject(new BxGyDetails
                {
                    BuyProducts = new List<BxGyProduct>
                    {
                        new BxGyProduct { ProductId = 1, Quantity = 2 }
                    },
                    GetProducts = new List<BxGyProduct>
                    {
                        new BxGyProduct { ProductId = 3, Quantity = 1 }
                    },
                    RepetitionLimit = 2
                })
            };

            var cart = new Cart
            {
                Items = new List<CartItem>
                {
                    new CartItem { ProductId = 1, Price = 50, Quantity = 4 },
                    new CartItem { ProductId = 3, Price = 25, Quantity = 1 }
                }
            };

            var strategy = new BxGyStrategy();
            var discount = strategy.CalculateDiscount(coupon, cart);

            Assert.AreEqual(50m, discount);
        }
    }
}