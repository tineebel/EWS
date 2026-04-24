namespace EWS.Application.Common.Interfaces;

/// <summary>
/// บริการเวลา — ทุก DateTime ในระบบใช้ UTC+7 (Thailand)
/// </summary>
public interface IDateTimeService
{
    DateTime Now { get; }
}
