using System;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class DateTimeService : IDateTimeService
    {
        private readonly ILogger<DateTimeService> _logger;

        public DateTimeService(ILogger<DateTimeService> logger)
        {
            _logger = logger;
        }

        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }

        public DateTime GetDateTimeUtcNow()
        {
            return DateTime.UtcNow;
        }

        public DateTime GetDateTimeFromUnixTimestamp(long exp)
        {
            try
            {
                var startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var dateTime = startDateTime.AddSeconds(exp);

                return dateTime;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(GetDateTimeFromUnixTimestamp));

                return DateTime.Now;
            }
        }

        public TimeZoneInfo GetLocalTimeZone()
        {
            return TimeZoneInfo.Local;
        }
    }
}
