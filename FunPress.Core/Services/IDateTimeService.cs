using System;

namespace FunPress.Core.Services
{
    public interface IDateTimeService
    {
        DateTime GetDateTimeNow();
        DateTime GetDateTimeUtcNow();
        DateTime GetDateTimeFromUnixTimestamp(long unixTimestamp);
        TimeZoneInfo GetLocalTimeZone();
    }
}
