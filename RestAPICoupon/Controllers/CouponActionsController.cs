using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RestAPICoupon.DTOs;
using RestAPICoupon.Models;
using RestAPICoupon.Repositories;
using RestAPICoupon.Services;

namespace RestAPICoupon.Controllers
{
    public class CouponActionsController : ApiController
    {
        private readonly CouponRepository _repo = new CouponRepository();
        private readonly CouponStrategyFactory _factory = new CouponStrategyFactory();

        // POST /applicable-coupons
        [HttpPost, Route("applicable-coupons")]
        public IHttpActionResult Applicable([FromBody] ApplicableCouponsRequest req)
        {
            if (req == null || req.Cart == null)
            {
                return BadRequest("Cart is required.");
            }

            var results = new List<ApplicableCouponResult>();

            // Evaluate each active coupon against the cart
            foreach (var c in _repo.GetAll())
            {
                if (!IsCouponActive(c))
                {
                    continue;
                }

                var strategy = _factory.Get(c.Type);
                if (strategy.IsApplicable(c, req.Cart))
                {
                    results.Add(new ApplicableCouponResult
                    {
                        CouponId = c.Id,
                        Type = c.Type,
                        Discount = strategy.CalculateDiscount(c, req.Cart)
                    });
                }
            }

            var response = new ApplicableCouponsResponse
            {
                ApplicableCoupons = results
            };

            return Ok(response);
        }

        // POST /apply-coupon/{id}
        [HttpPost, Route("apply-coupon/{id:int}")]
        public IHttpActionResult Apply(int id, [FromBody] ApplyCouponRequest req)
        {
            if (req == null || req.Cart == null)
            {
                return BadRequest("Cart is required.");
            }

            var c = _repo.GetById(id);

            if (c == null)
            {
                return NotFound();
            }

            if (!IsCouponActive(c))
            {
                return BadRequest("Coupon is inactive or expired.");
            }

            var strategy = _factory.Get(c.Type);

            if (!strategy.IsApplicable(c, req.Cart))
            {
                return BadRequest("Coupon not applicable for this cart.");
            }

            // Apply coupon and compute totals
            var updated = strategy.Apply(c, req.Cart);
            var totals = CalculateTotals(updated);

            var response = new ApplyCouponResponse
            {
                UpdatedCart = updated,
                TotalPrice = totals.TotalPrice,
                TotalDiscount = totals.TotalDiscount,
                FinalPrice = totals.FinalPrice
            };

            return Ok(response);
        }

        /// <summary>
        /// Determines if coupon is active based on flags and dates
        /// </summary>
        /// <param name="c">Coupon</param>
        /// <returns>true if coupon active else false</returns>
        private bool IsCouponActive(Coupon c)
        {
            if (!c.IsActive)
            {
                return false;
            }

            // Assume dates are stored in UTC
            var now = DateTime.UtcNow;

            if (c.StartDate.HasValue && c.StartDate.Value > now)
            {
                return false;
            }

            if (c.EndDate.HasValue && c.EndDate.Value < now)
            {
                return false;
            }

            // Note: MaxRedemptions logic is not enforced yet
            return true;
        }

        /// <summary>
        /// Calculates cart totals based on item price, quantity, and per-item discount
        /// </summary>
        /// <param name="cart"></param>
        /// <returns>Cart totals</returns>
        private (decimal TotalPrice, decimal TotalDiscount, decimal FinalPrice) CalculateTotals(Cart cart)
        {
            var totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            var totalDiscount = cart.Items.Sum(i => i.TotalDiscount);
            var finalPrice = totalPrice - totalDiscount;

            return (Math.Round(totalPrice, 2), Math.Round(totalDiscount, 2), Math.Round(finalPrice, 2));
        }
    }
}