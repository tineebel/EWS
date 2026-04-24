namespace EWS.Domain.Enums;

public enum WorkflowStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Escalated = 5,
    Recalled = 6,
    Blocked = 7      // Has at least one Stuck step — requires admin intervention
}
