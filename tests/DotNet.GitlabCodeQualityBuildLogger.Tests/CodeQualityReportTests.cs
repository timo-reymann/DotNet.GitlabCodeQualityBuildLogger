using System.Text.Json;

namespace DotNet.GitlabCodeQualityBuildLogger.Tests;

public class CodeQualityReportTests
{
    [Fact]
    public void ToJson_WithEmptyCollection_ReturnsEmptyArray()
    {
        var issues = Array.Empty<CodeQualityIssue>();

        var json = CodeQualityReport.ToJson(issues);

        Assert.Equal("[]", json);
    }

    [Fact]
    public void ToJson_WithSingleIssue_ReturnsValidJson()
    {
        var issues = new[]
        {
            CodeQualityIssue.Create("CS0001", "Test error", "file.cs", 10, "major")
        };

        var json = CodeQualityReport.ToJson(issues);

        var parsed = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, parsed.RootElement.ValueKind);
        Assert.Equal(1, parsed.RootElement.GetArrayLength());
    }

    [Fact]
    public void ToJson_ContainsExpectedProperties()
    {
        var issues = new[]
        {
            CodeQualityIssue.Create("CS0001", "Test error", "file.cs", 10, "major")
        };

        var json = CodeQualityReport.ToJson(issues);

        var parsed = JsonDocument.Parse(json);
        var firstIssue = parsed.RootElement[0];

        Assert.True(firstIssue.TryGetProperty("description", out _));
        Assert.True(firstIssue.TryGetProperty("check_name", out _));
        Assert.True(firstIssue.TryGetProperty("fingerprint", out _));
        Assert.True(firstIssue.TryGetProperty("severity", out _));
        Assert.True(firstIssue.TryGetProperty("location", out _));
    }

    [Fact]
    public void ToJson_LocationContainsPathAndLines()
    {
        var issues = new[]
        {
            CodeQualityIssue.Create("CS0001", "Test error", "file.cs", 10, "major")
        };

        var json = CodeQualityReport.ToJson(issues);

        var parsed = JsonDocument.Parse(json);
        var location = parsed.RootElement[0].GetProperty("location");

        Assert.True(location.TryGetProperty("path", out _));
        Assert.True(location.TryGetProperty("lines", out _));
    }

    [Fact]
    public void ToJson_LinesContainsBegin()
    {
        var issues = new[]
        {
            CodeQualityIssue.Create("CS0001", "Test error", "file.cs", 42, "major")
        };

        var json = CodeQualityReport.ToJson(issues);

        var parsed = JsonDocument.Parse(json);
        var lines = parsed.RootElement[0].GetProperty("location").GetProperty("lines");

        Assert.Equal(42, lines.GetProperty("begin").GetInt32());
    }

    [Fact]
    public void ToJson_WithMultipleIssues_ReturnsAllIssues()
    {
        var issues = new[]
        {
            CodeQualityIssue.Create("CS0001", "Error 1", "file1.cs", 10, "major"),
            CodeQualityIssue.Create("CS0002", "Error 2", "file2.cs", 20, "minor"),
            CodeQualityIssue.Create("CA1001", "Warning", "file3.cs", 30, "info")
        };

        var json = CodeQualityReport.ToJson(issues);

        var parsed = JsonDocument.Parse(json);
        Assert.Equal(3, parsed.RootElement.GetArrayLength());
    }

    [Fact]
    public void ToJson_UsesSnakeCasePropertyNames()
    {
        var issues = new[]
        {
            CodeQualityIssue.Create("CS0001", "Test error", "file.cs", 10, "major")
        };

        var json = CodeQualityReport.ToJson(issues);

        Assert.Contains("\"check_name\"", json);
        Assert.DoesNotContain("\"CheckName\"", json);
    }

    [Fact]
    public void ToJson_OutputIsNotIndented()
    {
        var issues = new[]
        {
            CodeQualityIssue.Create("CS0001", "Test error", "file.cs", 10, "major")
        };

        var json = CodeQualityReport.ToJson(issues);

        // Non-indented JSON should not contain newlines
        Assert.DoesNotContain("\n", json);
    }
}
