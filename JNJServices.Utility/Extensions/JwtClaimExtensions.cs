using JNJServices.Models.CommonModels;
using System.Security.Claims;
using System.Security.Principal;

namespace JNJServices.Utility.Extensions
{
    public static class JwtClaimExtensions
    {
        public static AppTokenDetails GetAppClaims(this IIdentity? identity)
        {
            AppTokenDetails claims = new AppTokenDetails();
            var claimsIdentity = identity as ClaimsIdentity;
            if (identity != null && claimsIdentity != null)
            {
                var cId = claimsIdentity.Claims.SingleOrDefault(x => x.Type == "Type");
                if (!string.IsNullOrEmpty(cId?.Value))
                {
                    claims.Type = Convert.ToInt32(cId.Value);
                }
                cId = claimsIdentity.Claims.SingleOrDefault(x => x.Type == "UserID");
                if (!string.IsNullOrEmpty(cId?.Value))
                {
                    claims.UserID = Convert.ToInt32(cId.Value);
                }
                cId = claimsIdentity.Claims.SingleOrDefault(x => x.Type == "PhoneNo");
                if (!string.IsNullOrEmpty(cId?.Value))
                {
                    claims.PhoneNo = cId.Value;
                }
                cId = claimsIdentity.Claims.SingleOrDefault(x => x.Type == "UserName");
                if (!string.IsNullOrEmpty(cId?.Value))
                {
                    claims.UserName = cId.Value;
                }
            }
            return claims;
        }

        public static string GetLoginUserId(this IIdentity? identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            if (identity != null && claimsIdentity != null)
            {
                var uId = claimsIdentity.Claims.SingleOrDefault(x => x.Type == "UserID");
                if (!string.IsNullOrEmpty(uId?.Value))
                {
                    return uId.Value.Trim();
                }
            }

            return string.Empty;
        }
    }
}
