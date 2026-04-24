namespace EWS.Application.Common;

/// <summary>
/// ประเมินเงื่อนไขใน WorkflowTemplate.Condition1..5
/// Format: "> 1000", "&lt;= 5000", "= 0", "NULL" (null/empty = always true)
/// </summary>
public static class WorkflowConditionEvaluator
{
    public static bool Evaluate(string? condition, decimal? amount)
    {
        if (string.IsNullOrWhiteSpace(condition) || condition.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return true;

        condition = condition.Trim();

        if (TryParse(condition, out var op, out var value))
        {
            var amt = amount ?? 0m;
            return op switch
            {
                ">=" => amt >= value,
                "<=" => amt <= value,
                ">"  => amt > value,
                "<"  => amt < value,
                "="  => amt == value,
                _    => true
            };
        }

        return true; // ถ้า parse ไม่ได้ → ถือว่าตรงเงื่อนไข (log warning แยก)
    }

    private static bool TryParse(string condition, out string op, out decimal value)
    {
        op = string.Empty;
        value = 0m;

        // ลอง match operator ยาวสุดก่อน (>=, <=) แล้วค่อย (>, <, =)
        foreach (var candidate in new[] { ">=", "<=", ">", "<", "=" })
        {
            if (!condition.StartsWith(candidate)) continue;

            var numPart = condition[candidate.Length..].Trim().Replace(",", "");
            if (!decimal.TryParse(numPart, out value)) continue;

            op = candidate;
            return true;
        }

        return false;
    }

    /// <summary>
    /// ตรวจสอบว่า condition string ถูกต้องหรือไม่ (ใช้ตอน Seed validation)
    /// </summary>
    public static bool IsValid(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition) || condition.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return true;

        return TryParse(condition.Trim(), out _, out _);
    }
}
