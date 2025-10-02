using Hangfire;
using Hangfire.SqlServer;
using JNJServices.API.Helper;
using JNJServices.API.Hubs;
using JNJServices.Models.CommonModels;
using JNJServices.Utility.ApiConstants;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using NLog.Web;
using System.Net;
using static JNJServices.Utility.UserResponses;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// Configure Logging
// ===========================
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// ===========================
// Add Services to DI Container
// ===========================
builder.Services.AddControllers();
// Register MemoryCache
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddApplicationServices();
builder.Services.AddAspApiVersioning();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationCors(builder.Configuration);

builder.Services.Configure<ApiVersionOptions>(builder.Configuration.GetSection("ApiVersionOptions"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<RaiseTicketEmail>(builder.Configuration.GetSection("RaiseTicketSetting"));
builder.Services.Configure<FirebaseSettings>(builder.Configuration.GetSection("Firebase"));

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddSignalR();

// ===========================
// Configure Hangfire
// ===========================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            PrepareSchemaIfNecessary = true,
            JobExpirationCheckInterval = TimeSpan.FromHours(1)
        });

    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
    {
        Attempts = 5,
        DelaysInSeconds = new[] { 60, 300, 600, 1800, 3600 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail
    });
});

builder.Services.AddHangfireServer();

// ===========================
// Build App
// ===========================
var app = builder.Build();

// ===========================
// Configure Middleware Pipeline
// ===========================

// Global exception handler (should be first to catch errors)
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var ex = context.Features.Get<IExceptionHandlerFeature>();

        var response = new
        {
            status = ResponseStatus.FALSE,
            statusMessage = ResponseMessage.DATA_NOT_FOUND,
            data = ex == null ? string.Empty : ex.Error.Message
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

// Use CORS
app.UseCors("AllowSpecificOrigins");

// HTTPS redirection & HSTS
if (builder.Configuration["ApplicationSetting:UseHttpsRedirection"] == "true")
{
    app.UseHttpsRedirection();
    app.UseHsts();
}


// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard();

// Static files
app.UseStaticFiles();

// Configure additional static directories
var staticDirs = new[]
{
    DefaultDirectorySettings.MediaFrontWayTripImage,
    DefaultDirectorySettings.MediaBackWayTripImage,
    DefaultDirectorySettings.MediaDeadMileImages,
    DefaultDirectorySettings.SettingImages,
    DefaultDirectorySettings.MediaContractorProfileImages,
    DefaultDirectorySettings.MediaAssignmentMetricsDocuments
};

foreach (var dir in staticDirs)
{
    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), dir);
    if (!Directory.Exists(fullPath))
        Directory.CreateDirectory(fullPath);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(fullPath),
        RequestPath = "/" + dir,
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=604800"; // 7 days
        }
    });
}

// Swagger (only in Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var groupName in descriptions.Select(x => x.GroupName))
        {
            options.SwaggerEndpoint($"/swagger/{groupName}/swagger.json", groupName.ToUpperInvariant());
        }
    });
}

// Map Controllers & Hubs
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

// ===========================
// Hangfire Recurring Jobs
// ===========================
app.Lifetime.ApplicationStarted.Register(() =>
{
    var cronHelper = new CronHelper(app.Services);
    var timeZone = app.Services.GetService<IConfiguration>().GetValue<string>("SelectTimeZone:TimeZone");
    var selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone ?? string.Empty);

    RecurringJob.AddOrUpdate("ContractorAssignment", () => cronHelper.NotificationContractorAssignment(), Cron.Daily(0, 1), new RecurringJobOptions { TimeZone = selectedTimeZone });
    RecurringJob.AddOrUpdate("ContractorNotAssigned", () => cronHelper.NotificationContractorNotAssigned(), Cron.Daily(0, 30), new RecurringJobOptions { TimeZone = selectedTimeZone });
    RecurringJob.AddOrUpdate("ContractorJobRequestandReminder", () => cronHelper.ContractorAssignmentjobRequestaAndNotification(), "*/30 * * * *", new RecurringJobOptions { TimeZone = selectedTimeZone });
    RecurringJob.AddOrUpdate("DeleteInactiveChats", () => cronHelper.CronDeleteInactiveChatRooms(), Cron.Daily(0, 5), new RecurringJobOptions { TimeZone = selectedTimeZone });
    RecurringJob.AddOrUpdate("DeleteNotification", () => cronHelper.CroneDeleteNotification(), Cron.Daily(0, 10), new RecurringJobOptions { TimeZone = selectedTimeZone });
    RecurringJob.AddOrUpdate("DeleteLiveCoordinate", () => cronHelper.CroneDeleteLiveCoordinates(), Cron.Daily(0, 15), new RecurringJobOptions { TimeZone = selectedTimeZone });
});

// ===========================
// Run App
// ===========================
await app.RunAsync();
