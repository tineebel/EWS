using EWS.Application.Common.Interfaces;

namespace EWS.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    private static readonly TimeZoneInfo ThailandTz =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ThailandTz);
}
