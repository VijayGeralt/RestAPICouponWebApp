using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using RestAPICoupon.DTOs;
using RestAPICoupon.Models;
using RestAPICoupon.Repositories;
using RestAPICoupon.Services;

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
            if (req == null)
            {
                return BadRequest("Invalid request body.");
            }

            if (string.IsNullOrWhiteSpace(req.Code))
            {
                return BadRequest("Code is required.");
            }

            if (!ValidateCommonFields(req.Type, req.DetailsJson, req.StartDate, req.EndDate, req.MaxRedemptions, out var error))
            {
                return BadRequest(error);
            }

            // Map request to domain model
            var coupon = new Coupon
            {
                Code = req.Code,
                Type = req.Type,
                DetailsJson = req.DetailsJson,
                IsActive = req.IsActive ?? true,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                MaxRedemptions = req.MaxRedemptions
            };

            try
            {
                var id = _repo.Create(coupon);
                return Ok<object>(new { id });
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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
            if (c == null)
            { 
                return NotFound(); 
            }

            return Ok(c);
        }

        // PUT /coupons/{id}
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] CouponUpdateRequest req)
        {
            if (req == null) return BadRequest("Invalid request body.");

            var existing = _repo.GetById(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!ValidateCommonFields(req.Type, req.DetailsJson, req.StartDate, req.EndDate, req.MaxRedemptions, out var error))
            {
                return BadRequest(error);
            }

            // Update mutable fields
            existing.Code = req.Code;
            existing.Type = req.Type;
            existing.DetailsJson = req.DetailsJson;
            existing.IsActive = req.IsActive;
            existing.StartDate = req.StartDate;
            existing.EndDate = req.EndDate;
            existing.MaxRedemptions = req.MaxRedemptions;

            try
            {
                var updated = _repo.Update(existing);
                return Ok<object>(new { updated });
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE /coupons/{id}
        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var deleted = _repo.Delete(id);
                if (!deleted)
                {
                    return NotFound();
                }

                return Ok<object>(new { deleted });
            }
            catch (SqlException ex)
            {
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // Validates common fields and DetailsJson
        private bool ValidateCommonFields(
            CouponType type,
            string detailsJson,
            DateTime? startDate,
            DateTime? endDate,
            int? maxRedemptions,
            out string error)
        {
            if (!CouponDetailsValidator.TryValidate(type, detailsJson, out error))
            {
                return false;
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                error = "StartDate must be before EndDate.";
                return false;
            }

            if (maxRedemptions.HasValue && maxRedemptions.Value <= 0)
            {
                error = "MaxRedemptions must be greater than 0.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
