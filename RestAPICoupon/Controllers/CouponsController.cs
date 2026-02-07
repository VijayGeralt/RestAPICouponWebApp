using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using RestAPICoupon.DTOs;
using RestAPICoupon.Models;
using RestAPICoupon.Repositories;

namespace RestAPICoupon.Controllers
{
    [RoutePrefix("coupons")]
    public class CouponsController : ApiController
    {
        private readonly CouponRepository _repo = new CouponRepository();

        // POST /coupons
        [HttpPost, Route("")]
        public IHttpActionResult Create([FromBody] CouponCreateRequest req)
        {
            if (req == null) return BadRequest("Invalid request body.");
            if (string.IsNullOrWhiteSpace(req.Code)) return BadRequest("Code is required.");

            // Map request to domain model
            var coupon = new Coupon
            {
                Code = req.Code,
                Type = req.Type,
                DetailsJson = req.DetailsJson,
                IsActive = true
            };

            var id = _repo.Create(coupon);
            return Ok(new { id });
        }

        // GET /coupons
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            return Ok(_repo.GetAll());
        }

        // GET /coupons/{id}
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var c = _repo.GetById(id);
            if (c == null) return NotFound();
            return Ok(c);
        }

        // PUT /coupons/{id}
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] CouponUpdateRequest req)
        {
            if (req == null) return BadRequest("Invalid request body.");

            var existing = _repo.GetById(id);
            if (existing == null) return NotFound();

            // Update mutable fields
            existing.Code = req.Code;
            existing.Type = req.Type;
            existing.DetailsJson = req.DetailsJson;
            existing.IsActive = req.IsActive;

            var updated = _repo.Update(existing);
            return Ok(new { updated });
        }

        // DELETE /coupons/{id}
        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var deleted = _repo.Delete(id);
            if (!deleted) return NotFound();
            return Ok(new { deleted });
        }
    }
}