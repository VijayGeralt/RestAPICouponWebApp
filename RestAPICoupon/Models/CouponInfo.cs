using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RestAPICoupon.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CouponType
    {
        CartWise,
        ProductWise,
        BxGy
    }
}
