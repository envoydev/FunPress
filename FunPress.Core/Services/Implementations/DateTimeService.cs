using System;

namespace FunPress.Core.Services.Implementations
{
    internal class DateTimeService : IDateTimeService
    {
        public DateTime GetDateTimeNow()
        {
            return DateTime.Now;
        }
    }
}
