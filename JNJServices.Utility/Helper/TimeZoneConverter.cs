using Microsoft.Extensions.Configuration;

namespace JNJServices.Utility.Helper
{
    public class TimeZoneConverter : ITimeZoneConverter
    {
        private readonly IConfiguration _configuration;

        public TimeZoneConverter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // A single method to convert UTC to the time zone configured in appsettings.json
        public DateTime ConvertUtcToConfiguredTimeZone()
        {
            DateTime utcDateTime = DateTime.UtcNow;
            // Get the time zone ID from appsettings.json
            string? timeZoneId = _configuration["SelectTimeZone:TimeZone"];

            if (string.IsNullOrEmpty(timeZoneId))
            {
                throw new ArgumentException("Time zone not found in configuration.");
            }

            // Find the time zone info using the configured time zone ID
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Convert the UTC DateTime to the configured time zone
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
    }
}
