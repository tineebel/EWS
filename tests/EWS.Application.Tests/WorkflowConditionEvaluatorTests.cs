using EWS.Application.Common;

namespace EWS.Application.Tests;

public class WorkflowConditionEvaluatorTests
{
    [Theory]
    [InlineData(null, 100, true)]
    [InlineData("", 100, true)]
    [InlineData("NULL", 100, true)]
    [InlineData("> 1000", 1001, true)]
    [InlineData("> 1000", 1000, false)]
    [InlineData("<= 5,000", 5000, true)]
    [InlineData("= 0", 0, true)]
    public void Evaluate_ValidConditions_ReturnsExpectedResult(string? condition, decimal amount, bool expected)
    {
        var result = WorkflowConditionEvaluator.Evaluate(condition, amount);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData(">> 1000")]
    [InlineData("> amount")]
    public void Evaluate_InvalidCondition_FailsClosed(string condition)
    {
        var result = WorkflowConditionEvaluator.Evaluate(condition, 999999);

        Assert.False(result);
    }

    [Theory]
    [InlineData("> 1000", true)]
    [InlineData("NULL", true)]
    [InlineData("> amount", false)]
    public void IsValid_ReportsConditionValidity(string condition, bool expected)
    {
        var result = WorkflowConditionEvaluator.IsValid(condition);

        Assert.Equal(expected, result);
    }
}
