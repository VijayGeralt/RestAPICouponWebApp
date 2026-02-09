using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestAPICoupon.Controllers;
using RestAPICoupon.DTOs;
using RestAPICoupon.Models;
using RestAPICoupon.Repositories;
using RestAPICoupon.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace RestAPICoupon.Tests
{
    [TestClass]
    public class CouponApiIntegrationTests
    {
        private static string UniqueCode(string prefix)
        {
            return $"{prefix}_{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
        }

        private static T SetupController<T>(T controller) where T : ApiController
        {
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
            return controller;
        }

        [TestMethod]
        public void Create_InvalidDetailsJson_ReturnsBadRequest()
        {
            var controller = SetupController(new CouponsController());
            var req = new CouponCreateRequest
            {
                Code = UniqueCode("BADJSON"),
                Type = CouponType.CartWise,
                DetailsJson = "{bad json"
            };

            var result = controller.Create(req);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Create_Valid_ReturnsId()
        {
            var controller = SetupController(new CouponsController());
            var details = JsonConvert.SerializeObject(new CartWiseDetails
            {
                MinCartTotal = 100,
                DiscountPercent = 10
            });

            var req = new CouponCreateRequest
            {
                Code = UniqueCode("CREATE"),
                Type = CouponType.CartWise,
                DetailsJson = details
            };

            var result = controller.Create(req) as OkNegotiatedContentResult<object>;
            Assert.IsNotNull(result);

            var idProp = result.Content.GetType().GetProperty("id");
            Assert.IsNotNull(idProp);
            var id = (int)idProp.GetValue(result.Content);
            Assert.IsTrue(id > 0);

            var repo = new CouponRepository();
            try
            {
                var created = repo.GetById(id);
                Assert.IsNotNull(created);
            }
            finally
            {
                repo.Delete(id);
            }
        }

        [TestMethod]
        public void Repository_Create_Update_Delete_Works()
        {
            var repo = new CouponRepository();
            var code = UniqueCode("CRUD");
            var details = JsonConvert.SerializeObject(new CartWiseDetails
            {
                MinCartTotal = 100,
                DiscountPercent = 10
            });

            var coupon = new Coupon
            {
                Code = code,
                Type = CouponType.CartWise,
                DetailsJson = details,
                IsActive = true
            };

            var id = repo.Create(coupon);
            Assert.IsTrue(id > 0);

            var created = repo.GetById(id);
            Assert.IsNotNull(created);
            Assert.AreEqual(code, created.Code);

            created.Code = code + "_UPD";
            var updated = repo.Update(created);
            Assert.IsTrue(updated);

            var fetched = repo.GetById(id);
            Assert.IsNotNull(fetched);
            Assert.AreEqual(code + "_UPD", fetched.Code);

            var deleted = repo.Delete(id);
            Assert.IsTrue(deleted);

            var afterDelete = repo.GetById(id);
            Assert.IsNull(afterDelete);
        }

        [TestMethod]
        public void Apply_BxGy_IncrementsRedemptions_And_Deactivates_When_MaxReached()
        {
            var repo = new CouponRepository();
            var code = UniqueCode("BXGY");

            var details = new BxGyDetails
            {
                BuyProducts = new List<BxGyProduct>
                {
                    new BxGyProduct { ProductId = 1, Quantity = 2 }
                },
                GetProducts = new List<BxGyProduct>
                {
                    new BxGyProduct { ProductId = 3, Quantity = 1 }
                },
                RepetitionLimit = 1
            };

            var coupon = new Coupon
            {
                Code = code,
                Type = CouponType.BxGy,
                DetailsJson = JsonConvert.SerializeObject(details),
                IsActive = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                MaxRedemptions = 1
            };

            var id = repo.Create(coupon);
            try
            {
                var controller = SetupController(new CouponActionsController());
                var req = new ApplyCouponRequest
                {
                    Cart = new Cart
                    {
                        Items = new List<CartItem>
                        {
                            new CartItem { ProductId = 1, Quantity = 2, Price = 50 },
                            new CartItem { ProductId = 3, Quantity = 1, Price = 25 }
                        }
                    }
                };

                var result = controller.Apply(id, req) as OkNegotiatedContentResult<ApplyCouponResponse>;
                Assert.IsNotNull(result);

                var updated = repo.GetById(id);
                Assert.IsNotNull(updated);
                Assert.AreEqual(1, updated.CurrentRedemptions);
                Assert.IsFalse(updated.IsActive);
            }
            finally
            {
                repo.Delete(id);
            }
        }
    }
}
