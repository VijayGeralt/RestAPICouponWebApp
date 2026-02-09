using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using RestAPICoupon.Models;
using System.Linq;
using System.Web;

namespace RestAPICoupon.Repositories
{
    public class CouponRepository
    {
        private readonly string _cs;

        public CouponRepository()
        {
            // Reads the connection string from Web.config
            _cs = ConfigurationManager.ConnectionStrings["CouponsDb"].ConnectionString;
        }

        // Inserts a new coupon row and returns the generated Id
        public int Create(Coupon coupon)
        {
            // Insert coupon and return identity in a single round-trip
            const string sql = @"
                                INSERT INTO dbo.Coupons
                                (Code, Type, DetailsJson, IsActive, StartDate, EndDate, MaxRedemptions, CurrentRedemptions)
                                VALUES
                                (@Code, @Type, @DetailsJson, @IsActive, @StartDate, @EndDate, @MaxRedemptions, 0);
                                SELECT SCOPE_IDENTITY();";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                // Parameterized to prevent SQL injection
                cmd.Parameters.AddWithValue("@Code", coupon.Code);
                cmd.Parameters.AddWithValue("@Type", coupon.Type.ToString());
                cmd.Parameters.AddWithValue("@DetailsJson", coupon.DetailsJson);
                cmd.Parameters.AddWithValue("@IsActive", coupon.IsActive);
                cmd.Parameters.AddWithValue("@StartDate", (object)coupon.StartDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object)coupon.EndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxRedemptions", (object)coupon.MaxRedemptions ?? DBNull.Value);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Maps a data row to a Coupon model
        private Coupon MapCoupon(IDataRecord r)
        {
            return new Coupon
            {
                Id = (int)r["Id"],
                Code = (string)r["Code"],
                Type = (CouponType)Enum.Parse(typeof(CouponType), (string)r["Type"], true),
                DetailsJson = (string)r["DetailsJson"],
                IsActive = (bool)r["IsActive"],
                StartDate = r["StartDate"] as DateTime?,
                EndDate = r["EndDate"] as DateTime?,
                MaxRedemptions = r["MaxRedemptions"] as int?,
                CurrentRedemptions = (int)r["CurrentRedemptions"]
            };
        }

        // Returns a single coupon by Id, or null if not found
        public Coupon GetById(int id)
        {
            // Select by primary key
            const string sql = "SELECT * FROM dbo.Coupons WHERE Id = @Id;";
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return MapCoupon(rdr);
                }
            }
        }

        // Returns all coupons
        public List<Coupon> GetAll()
        {
            // Simple select for all rows
            const string sql = "SELECT * FROM dbo.Coupons;";
            var list = new List<Coupon>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        // Map each row into a Coupon model
                        list.Add(MapCoupon(rdr));
                    }
                }
            }
            return list;
        }

        // Updates an existing coupon; returns true if a row was affected
        public bool Update(Coupon coupon)
        {
            // Update all editable fields
            const string sql = @"
                                UPDATE dbo.Coupons
                                SET Code = @Code,
                                Type = @Type,
                                DetailsJson = @DetailsJson,
                                IsActive = @IsActive,
                                StartDate = @StartDate,
                                EndDate = @EndDate,
                                MaxRedemptions = @MaxRedemptions,
                                UpdatedAt = SYSUTCDATETIME()
                                WHERE Id = @Id;";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", coupon.Id);
                cmd.Parameters.AddWithValue("@Code", coupon.Code);
                cmd.Parameters.AddWithValue("@Type", coupon.Type.ToString());
                cmd.Parameters.AddWithValue("@DetailsJson", coupon.DetailsJson);
                cmd.Parameters.AddWithValue("@IsActive", coupon.IsActive);
                cmd.Parameters.AddWithValue("@StartDate", (object)coupon.StartDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object)coupon.EndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxRedemptions", (object)coupon.MaxRedemptions ?? DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Deletes a coupon by Id; returns true if a row was deleted
        public bool Delete(int id)
        {
            // Delete by primary key
            const string sql = "DELETE FROM dbo.Coupons WHERE Id = @Id;";
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool IncrementRedemptionsAndDeactivateIfNeeded(int id)
        {
            const string sql = @"
                                UPDATE dbo.Coupons
                                SET CurrentRedemptions = CurrentRedemptions + 1,
                                    IsActive = CASE 
                                        WHEN MaxRedemptions IS NOT NULL AND CurrentRedemptions + 1 >= MaxRedemptions THEN 0 
                                        ELSE IsActive 
                                    END,
                                    UpdatedAt = SYSUTCDATETIME()
                                WHERE Id = @Id
                                  AND IsActive = 1
                                  AND (MaxRedemptions IS NULL OR CurrentRedemptions < MaxRedemptions);";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();

                // true if current redemptions updated successfully, else false
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}