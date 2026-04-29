using EWS.Domain.Common;

namespace EWS.Domain.Entities;

public class WorkflowDocumentSequence : BaseEntity
{
    public int SequenceId { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public int Year { get; set; }
    public int LastNumber { get; set; }
}
