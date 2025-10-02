using JNJServices.Business.Abstracts;
using JNJServices.Models.ApiResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Attributes
{
    /// <summary>
    /// Used to Validate Single user login for mobile application users
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ValidateAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                ResponseModel response = new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = ResponseMessage.UNAUTHORIZE,
                    data = ResponseMessage.INVALID_USER_CREDENTIALS
                };

                var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
                if (allowAnonymous)
                    return;

                var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    context.Result = new JsonResult(response)
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                    return;
                }

                var token = authHeader?.Substring("Bearer ".Length).Trim();
                var userTypeClaim = context.HttpContext.User.FindFirst("Type");
                var userIdClaim = context.HttpContext.User.FindFirst("UserID");

                if (userTypeClaim == null || userIdClaim == null)
                {
                    context.Result = new UnauthorizedObjectResult(response);
                    return;
                }

                if (!int.TryParse(userTypeClaim.Value, out var userType) ||
                    !int.TryParse(userIdClaim.Value, out var userId))
                {
                    context.Result = new UnauthorizedObjectResult(response);
                    return;
                }

                // Get MemoryCache
                var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                string cacheKey = $"auth_{userType}_{userId}_{token}";

                if (!cache.TryGetValue(cacheKey, out bool isValid))
                {
                    // Resolve the IUsers service from the service provider
                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();

                    // Perform the authorization check
                    var user = userService.ValidateUserAuthorization(userType, userId, token ?? string.Empty).Result;

                    isValid = user != 0;

                    // Cache result for 10 minutes
                    cache.Set(cacheKey, isValid, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    });
                }

                if (!isValid)
                {
                    context.Result = new UnauthorizedObjectResult(response)
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
            }
            catch (Exception)
            {
                ResponseModel response = new ResponseModel
                {
                    status = ResponseStatus.FALSE,
                    statusMessage = ResponseMessage.UNAUTHORIZE,
                    data = ResponseMessage.INVALID_USER_CREDENTIALS
                };

                context.Result = new JsonResult(response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}
