using Microsoft.Build.Framework;
using Moq;

namespace DotNet.GitlabCodeQualityBuildLogger.Tests;

public class WarningSeverityTests : IDisposable
{
    private readonly Mock<IEventSource> _eventSourceMock;
    private readonly CustomBuildLogger _logger;
    private readonly string _tempFile;

    public WarningSeverityTests()
    {
        _eventSourceMock = new Mock<IEventSource>();
        _logger = new CustomBuildLogger();
        _tempFile = Path.GetTempFileName();
        _logger.Parameters = _tempFile;
        _logger.Initialize(_eventSourceMock.Object);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    private void RaiseWarning(string code)
    {
        _eventSourceMock.Raise(e => e.WarningRaised += null,
            new BuildWarningEventArgs(null, code, "test.cs", 1, 1, 1, 1, "Test", null, "test"));
    }

    private string GetOutputContent()
    {
        _logger.Shutdown();
        return File.ReadAllText(_tempFile);
    }

    [Theory]
    [InlineData("CA1001")]
    [InlineData("CA1234")]
    [InlineData("ca1001")] // lowercase
    public void CAWarnings_HaveMinorSeverity(string code)
    {
        RaiseWarning(code);

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"minor\"", content);
    }

    [Theory]
    [InlineData("CA2000")] // Security - disposing
    [InlineData("CA2100")] // Security - SQL injection
    [InlineData("CA2345")]
    [InlineData("ca2001")] // lowercase
    public void CA2SecurityWarnings_HaveMajorSeverity(string code)
    {
        RaiseWarning(code);

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"major\"", content);
    }

    [Theory]
    [InlineData("CA5000")]
    [InlineData("CA5350")] // Weak cryptography
    [InlineData("CA5351")]
    [InlineData("ca5000")] // lowercase
    public void CA5SecurityWarnings_HaveMajorSeverity(string code)
    {
        RaiseWarning(code);

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"major\"", content);
    }

    [Theory]
    [InlineData("CS0168")] // Variable declared but never used
    [InlineData("CS0219")] // Variable assigned but never used
    [InlineData("CS8600")] // Null warning
    [InlineData("cs0001")] // lowercase
    public void CSCompilerWarnings_HaveMajorSeverity(string code)
    {
        RaiseWarning(code);

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"major\"", content);
    }

    [Theory]
    [InlineData("IDE0001")]
    [InlineData("IDE0051")]
    [InlineData("ide0001")] // lowercase
    public void IDEWarnings_HaveInfoSeverity(string code)
    {
        RaiseWarning(code);

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"info\"", content);
    }

    [Theory]
    [InlineData("SA1001")]
    [InlineData("SA1600")]
    [InlineData("sa1001")] // lowercase
    public void SAStyleCopWarnings_HaveInfoSeverity(string code)
    {
        RaiseWarning(code);

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"info\"", content);
    }

    [Theory]
    [InlineData("NU1001")] // NuGet
    [InlineData("MSB1001")] // MSBuild
    [InlineData("UNKNOWN")]
    [InlineData("")]
    public void OtherWarnings_HaveMinorSeverity(string code)
    {
        RaiseWarning(code);

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"minor\"", content);
    }

    [Fact]
    public void ErrorsAlways_HaveCriticalSeverity()
    {
        _eventSourceMock.Raise(e => e.ErrorRaised += null,
            new BuildErrorEventArgs(null, "CS0001", "test.cs", 1, 1, 1, 1, "Error", null, "test"));

        var content = GetOutputContent();

        Assert.Contains("\"severity\":\"critical\"", content);
    }
}
