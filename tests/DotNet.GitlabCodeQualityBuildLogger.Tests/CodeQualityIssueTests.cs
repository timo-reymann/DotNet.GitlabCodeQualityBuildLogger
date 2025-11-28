namespace DotNet.GitlabCodeQualityBuildLogger.Tests;

public class CodeQualityIssueTests
{
    [Fact]
    public void Create_SetsDescriptionWithCheckNameAndMessage()
    {
        var issue = CodeQualityIssue.Create("CS0001", "Test message", "file.cs", 10, "major");

        Assert.Equal("CS0001: Test message", issue.Description);
    }

    [Fact]
    public void Create_SetsCheckName()
    {
        var issue = CodeQualityIssue.Create("CA1001", "Some warning", "file.cs", 5, "minor");

        Assert.Equal("CA1001", issue.CheckName);
    }

    [Fact]
    public void Create_SetsSeverity()
    {
        var issue = CodeQualityIssue.Create("IDE0001", "Suggestion", "file.cs", 1, "info");

        Assert.Equal("info", issue.Severity);
    }

    [Fact]
    public void Create_SetsLocationPath()
    {
        var issue = CodeQualityIssue.Create("CS0001", "Error", "src/MyClass.cs", 20, "critical");

        Assert.NotNull(issue.Location);
        Assert.NotNull(issue.Location.Path);
    }

    [Fact]
    public void Create_SetsLocationLineNumber()
    {
        var issue = CodeQualityIssue.Create("CS0001", "Error", "file.cs", 42, "major");

        Assert.Equal(42, issue.Location.Lines.Begin);
    }

    [Fact]
    public void Create_WithZeroLineNumber_SetsLineToOne()
    {
        var issue = CodeQualityIssue.Create("CS0001", "Error", "file.cs", 0, "major");

        Assert.Equal(1, issue.Location.Lines.Begin);
    }

    [Fact]
    public void Create_WithNegativeLineNumber_SetsLineToOne()
    {
        var issue = CodeQualityIssue.Create("CS0001", "Error", "file.cs", -5, "major");

        Assert.Equal(1, issue.Location.Lines.Begin);
    }

    [Fact]
    public void Create_GeneratesConsistentFingerprint()
    {
        var issue1 = CodeQualityIssue.Create("CS0001", "Error", "file.cs", 10, "major");
        var issue2 = CodeQualityIssue.Create("CS0001", "Error", "file.cs", 10, "major");

        Assert.Equal(issue1.Fingerprint, issue2.Fingerprint);
    }

    [Fact]
    public void Create_GeneratesDifferentFingerprintForDifferentInputs()
    {
        var issue1 = CodeQualityIssue.Create("CS0001", "Error", "file.cs", 10, "major");
        var issue2 = CodeQualityIssue.Create("CS0002", "Error", "file.cs", 10, "major");

        Assert.NotEqual(issue1.Fingerprint, issue2.Fingerprint);
    }

    [Fact]
    public void Create_FingerprintIsValidHexString()
    {
        var issue = CodeQualityIssue.Create("CS0001", "Error", "file.cs", 10, "major");

        // MD5 produces 32 hex characters
        Assert.Equal(32, issue.Fingerprint.Length);
        Assert.True(issue.Fingerprint.All(c => Uri.IsHexDigit(c)));
    }

    [Fact]
    public void Create_WithEmptyPath_SetsPathToUnknown()
    {
        var issue = CodeQualityIssue.Create("CS0001", "Error", "", 10, "major");

        Assert.Equal("unknown", issue.Location.Path);
    }

    [Fact]
    public void Create_NormalizesBackslashesToForwardSlashes()
    {
        // This tests the path normalization behavior
        // The actual path will be relative to current directory
        var issue = CodeQualityIssue.Create("CS0001", "Error", "src\\subfolder\\file.cs", 10, "major");

        Assert.DoesNotContain("\\", issue.Location.Path);
    }
}
