using System.Linq;
using Newtonsoft.Json;
using RestAPICoupon.Models;

namespace RestAPICoupon.Services
{
    // Validates DetailsJson according to coupon type rules
    public static class CouponDetailsValidator
    {
        public static bool TryValidate(CouponType type, string detailsJson, out string error)
        {
            if (string.IsNullOrWhiteSpace(detailsJson))
            {
                error = "DetailsJson is required.";
                return false;
            }

            try
            {
                switch (type)
                {
                    case CouponType.CartWise:
                        var cart = JsonConvert.DeserializeObject<CartWiseDetails>(detailsJson);
                        if (cart == null)
                        {
                            error = "Cart-wise details are required.";
                            return false;
                        }
                        if (cart.MinCartTotal < 0)
                        {
                            error = "MinCartTotal must be >= 0.";
                            return false;
                        }
                        if (cart.DiscountPercent <= 0 || cart.DiscountPercent > 100)
                        {
                            error = "DiscountPercent must be between 0 and 100.";
                            return false;
                        }
                        if (cart.MaxDiscount.HasValue && cart.MaxDiscount.Value < 0)
                        {
                            error = "MaxDiscount must be >= 0.";
                            return false;
                        }
                        break;

                    case CouponType.ProductWise:
                        var product = JsonConvert.DeserializeObject<ProductWiseDetails>(detailsJson);
                        if (product == null)
                        {
                            error = "Product-wise details are required.";
                            return false;
                        }
                        if (product.ProductId <= 0)
                        {
                            error = "ProductId must be > 0.";
                            return false;
                        }
                        if (product.DiscountPercent <= 0 || product.DiscountPercent > 100)
                        {
                            error = "DiscountPercent must be between 0 and 100.";
                            return false;
                        }
                        if (product.MaxDiscount.HasValue && product.MaxDiscount.Value < 0)
                        {
                            error = "MaxDiscount must be >= 0.";
                            return false;
                        }
                        if (product.MinCartTotal.HasValue && product.MinCartTotal.Value < 0)
                        {
                            error = "MinCartTotal must be >= 0.";
                            return false;
                        }
                        break;

                    case CouponType.BxGy:
                        var bxgy = JsonConvert.DeserializeObject<BxGyDetails>(detailsJson);
                        if (bxgy == null)
                        {
                            error = "BxGy details are required.";
                            return false;
                        }
                        if (bxgy.BuyProducts == null || bxgy.BuyProducts.Count == 0)
                        {
                            error = "BuyProducts must have at least one item.";
                            return false;
                        }
                        if (bxgy.GetProducts == null || bxgy.GetProducts.Count == 0)
                        {
                            error = "GetProducts must have at least one item.";
                            return false;
                        }
                        if (bxgy.BuyProducts.Any(p => p.ProductId <= 0 || p.Quantity <= 0))
                        {
                            error = "BuyProducts items must have ProductId > 0 and Quantity > 0.";
                            return false;
                        }
                        if (bxgy.GetProducts.Any(p => p.ProductId <= 0 || p.Quantity <= 0))
                        {
                            error = "GetProducts items must have ProductId > 0 and Quantity > 0.";
                            return false;
                        }
                        if (bxgy.RepetitionLimit < 0)
                        {
                            error = "RepetitionLimit must be >= 0.";
                            return false;
                        }
                        if (bxgy.MinCartTotal.HasValue && bxgy.MinCartTotal.Value < 0)
                        {
                            error = "MinCartTotal must be >= 0.";
                            return false;
                        }
                        break;

                    default:
                        error = "Unsupported coupon type.";
                        return false;
                }
            }
            catch (JsonException ex)
            {
                error = "Invalid DetailsJson: " + ex.Message;
                return false;
            }

            error = null;
            return true;
        }
    }
}