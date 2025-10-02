using Asp.Versioning;
using JNJServices.API.Hubs;
using JNJServices.Business.Abstracts;
using JNJServices.Business.Email;
using JNJServices.Business.Services;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Utility.Helper;
using JNJServices.Utility.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using static JNJServices.Utility.UserResponses;

namespace JNJServices.API.Helper
{
    public static class RegisterServiceConfig
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<TimeZoneConverter>();
            services.AddSingleton<ITimeZoneConverter, TimeZoneConverter>();
            services.AddSingleton<IRoomConnectionMapping, RoomConnectionMapping>();
            services.AddSingleton<IConnectionMapping, ConnectionMapping>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IContractorService, ContractorService>();
            services.AddScoped<IClaimantService, ClaimantService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IReservationAssignmentsService, ReservationAssignmentsService>();
            services.AddScoped<IAssignmentMetricsService, AssignmentMetricsService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IClaimsService, ClaimsService>();
            services.AddScoped<IMiscellaneousService, MiscellaneousService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IJwtFactory, JwtFactory>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<NotificationHelper>();
            services.AddScoped<INotificationHelper, NotificationHelper>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IDapperContext, DapperContext>();
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();
            services.AddHostedService<NotificationWorker>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            return services;
        }

        public static IServiceCollection AddAspApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"));
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            string schemeType = "Bearer";
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition(schemeType, new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header ",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = schemeType
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                       {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                Type = ReferenceType.SecurityScheme,
                                Id = schemeType
                                },
                                Scheme = "oauth2",
                                Name = schemeType,
                                In = ParameterLocation.Header,

                            },new List<string>()
                        }
                });
            });

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfigurationRoot configuration)
        {
            string key = configuration["Jwt:Key"] ?? string.Empty;
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Call this to skip the default logic and avoid using the default response
                        context.HandleResponse();
                        ResponseModel response = new ResponseModel();
                        response.status = ResponseStatus.FALSE;
                        response.statusMessage = ResponseMessage.UNAUTHORIZE;
                        response.data = ResponseMessage.INVALID_USER_CREDENTIALS;
                        // Write to the response in any way you wish
                        context.Response.StatusCode = 401;
                        context.Response.Headers.Append("my-custom-header", "custom-value");
                        await context.Response.WriteAsJsonAsync(response);
                    },

                    OnMessageReceived = context =>
                    {
                        // ✅ Allow token via query string (SignalR style)
                        var accessToken = context.Request.Query["access_token"];

                        // ✅ Only for SignalR hub route
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
            return services;
        }

        public static IServiceCollection AddApplicationCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowSpecificOrigins = "AllowSpecificOrigins";
            var originList = configuration.GetValue<string>("CORSSetting:AllowOriginList");

            services.AddCors(options =>
            {
                if (!string.IsNullOrWhiteSpace(originList))
                {
                    var originListArray = originList.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(o => o.Trim()) // Trim spaces to avoid issues
                                                    .ToArray();

                    options.AddPolicy(allowSpecificOrigins, builder =>
                    {
                        builder.WithOrigins(originListArray)
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials() // Allow credentials (cookies, authorization headers)
                               .SetPreflightMaxAge(TimeSpan.FromHours(1));
                    });
                }
            });

            return services;
        }

    }
}
