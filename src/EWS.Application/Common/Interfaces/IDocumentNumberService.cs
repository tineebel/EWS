namespace EWS.Application.Common.Interfaces;

public interface IDocumentNumberService
{
    /// <summary>
    /// สร้างเลขที่เอกสาร Running Number
    /// Format: {prefix}-{YYYY}-{5digits} เช่น PCV-2026-00001
    /// </summary>
    Task<string> GenerateAsync(int docCode, CancellationToken ct = default);
}
