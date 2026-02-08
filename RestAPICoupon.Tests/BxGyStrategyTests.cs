using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestAPICoupon.Models;
using RestAPICoupon.Services;
using System.Collections.Generic;

namespace RestAPICoupon.Tests
{
    [TestClass]
    public class BxGyStrategyTests
    {
        private Coupon CreateCoupon(BxGyDetails details)
        {
            return new Coupon
            {
                Type = CouponType.BxGy,
                DetailsJson = JsonConvert.SerializeObject(details)
            };
        }

        private Cart CreateCart(params CartItem[] items)
        {
            return new Cart { Items = new List<CartItem>(items) };
        }

        [TestMethod]
        public void IsApplicable_False_WhenMinCartTotalNotMet()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                MinCartTotal = 300,
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 2 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 2
            });

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 2 }, // total 100
                new CartItem { ProductId = 3, Price = 25, Quantity = 1 }  // total 25
            );

            var strategy = new BxGyStrategy();
            Assert.IsFalse(strategy.IsApplicable(coupon, cart));
        }

        [TestMethod]
        public void IsApplicable_False_WhenBuyRequirementsNotMet()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 3 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 2
            });

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 2 },
                new CartItem { ProductId = 3, Price = 25, Quantity = 1 }
            );

            var strategy = new BxGyStrategy();
            Assert.IsFalse(strategy.IsApplicable(coupon, cart));
        }

        [TestMethod]
        public void IsApplicable_False_WhenGetProductMissing()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 2 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 2
            });

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 2 }
            );

            var strategy = new BxGyStrategy();
            Assert.IsFalse(strategy.IsApplicable(coupon, cart));
        }

        [TestMethod]
        public void IsApplicable_True_WhenAllConditionsMet()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 2 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 2
            });

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 4 },
                new CartItem { ProductId = 3, Price = 25, Quantity = 1 }
            );

            var strategy = new BxGyStrategy();
            Assert.IsTrue(strategy.IsApplicable(coupon, cart));
        }

        [TestMethod]
        public void CalculateDiscount_RespectsRepetitionLimit()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 2 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 1
            });

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 10 },
                new CartItem { ProductId = 3, Price = 25, Quantity = 1 }
            );

            var strategy = new BxGyStrategy();
            var discount = strategy.CalculateDiscount(coupon, cart);

            Assert.AreEqual(25m, discount);
        }

        [TestMethod]
        public void CalculateDiscount_AllowsUnlimited_WhenRepetitionLimitZero()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 2 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 0
            });

            coupon.StartDate = System.DateTime.UtcNow;
            coupon.EndDate = new System.DateTime(2026, 02, 23, 00, 00, 00);

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 4 },
                new CartItem { ProductId = 3, Price = 25, Quantity = 1 }
            );

            var strategy = new BxGyStrategy();
            var discount = strategy.CalculateDiscount(coupon, cart);

            Assert.AreEqual(50m, discount); // 2 free items * 25
        }

        [TestMethod]
        public void Apply_AddsFreeItemsAndDiscount()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 2 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 2
            });

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 4 },
                new CartItem { ProductId = 3, Price = 25, Quantity = 1 }
            );

            var strategy = new BxGyStrategy();
            var updated = strategy.Apply(coupon, cart);

            var freeItem = updated.Items.Find(i => i.ProductId == 3);
            Assert.AreEqual(3, freeItem.Quantity);     // +2 free
            Assert.AreEqual(50m, freeItem.TotalDiscount);
        }

        [TestMethod]
        public void Apply_DoesNothing_WhenNoEligibleSets()
        {
            var coupon = CreateCoupon(new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 1, Quantity = 2 } },
                GetProducts = new List<BxGyProduct> { new BxGyProduct { ProductId = 3, Quantity = 1 } },
                RepetitionLimit = 2
            });

            var cart = CreateCart(
                new CartItem { ProductId = 1, Price = 50, Quantity = 1 },
                new CartItem { ProductId = 3, Price = 25, Quantity = 1 }
            );

            var strategy = new BxGyStrategy();
            var updated = strategy.Apply(coupon, cart);

            var freeItem = updated.Items.Find(i => i.ProductId == 3);
            Assert.AreEqual(1, freeItem.Quantity);
            Assert.AreEqual(0m, freeItem.TotalDiscount);
        }
    }
}