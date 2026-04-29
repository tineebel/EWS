using System.Globalization;

namespace EWS.Application.Common;

/// <summary>
/// Evaluates WorkflowTemplate.Condition1..5.
/// Supported format: "&gt; 1000", "&lt;= 5000", "= 0", "NULL".
/// Null or empty conditions match; invalid condition strings do not match.
/// </summary>
public static class WorkflowConditionEvaluator
{
    public static bool Evaluate(string? condition, decimal? amount)
    {
        if (string.IsNullOrWhiteSpace(condition) || condition.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return true;

        if (!TryParse(condition.Trim(), out var op, out var value))
            return false;

        var amt = amount ?? 0m;
        return op switch
        {
            ">=" => amt >= value,
            "<=" => amt <= value,
            ">"  => amt > value,
            "<"  => amt < value,
            "="  => amt == value,
            _    => false
        };
    }

    private static bool TryParse(string condition, out string op, out decimal value)
    {
        op = string.Empty;
        value = 0m;

        foreach (var candidate in new[] { ">=", "<=", ">", "<", "=" })
        {
            if (!condition.StartsWith(candidate, StringComparison.Ordinal)) continue;

            var numPart = condition[candidate.Length..].Trim().Replace(",", "");
            if (!decimal.TryParse(numPart, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                continue;

            op = candidate;
            return true;
        }

        return false;
    }

    public static bool IsValid(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition) || condition.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return true;

        return TryParse(condition.Trim(), out _, out _);
    }
}
