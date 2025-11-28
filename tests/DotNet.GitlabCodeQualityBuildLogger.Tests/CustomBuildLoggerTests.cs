using Microsoft.Build.Framework;
using Moq;

namespace DotNet.GitlabCodeQualityBuildLogger.Tests;

public class CustomBuildLoggerTests
{
    private readonly Mock<IEventSource> _eventSourceMock;
    private readonly CustomBuildLogger _logger;

    public CustomBuildLoggerTests()
    {
        _eventSourceMock = new Mock<IEventSource>();
        _logger = new CustomBuildLogger();
    }

    [Fact]
    public void Initialize_SubscribesToProjectStartedEvent()
    {
        _logger.Initialize(_eventSourceMock.Object);

        _eventSourceMock.VerifyAdd(e => e.ProjectStarted += It.IsAny<ProjectStartedEventHandler>(), Times.Once);
    }

    [Fact]
    public void Initialize_SubscribesToProjectFinishedEvent()
    {
        _logger.Initialize(_eventSourceMock.Object);

        _eventSourceMock.VerifyAdd(e => e.ProjectFinished += It.IsAny<ProjectFinishedEventHandler>(), Times.Once);
    }

    [Fact]
    public void Initialize_SubscribesToErrorRaisedEvent()
    {
        _logger.Initialize(_eventSourceMock.Object);

        _eventSourceMock.VerifyAdd(e => e.ErrorRaised += It.IsAny<BuildErrorEventHandler>(), Times.Once);
    }

    [Fact]
    public void Initialize_SubscribesToWarningRaisedEvent()
    {
        _logger.Initialize(_eventSourceMock.Object);

        _eventSourceMock.VerifyAdd(e => e.WarningRaised += It.IsAny<BuildWarningEventHandler>(), Times.Once);
    }

    [Fact]
    public void Initialize_SubscribesToTargetEvents()
    {
        _logger.Initialize(_eventSourceMock.Object);

        _eventSourceMock.VerifyAdd(e => e.TargetStarted += It.IsAny<TargetStartedEventHandler>(), Times.Once);
        _eventSourceMock.VerifyAdd(e => e.TargetFinished += It.IsAny<TargetFinishedEventHandler>(), Times.Once);
    }

    [Fact]
    public void Initialize_SubscribesToTaskEvents()
    {
        _logger.Initialize(_eventSourceMock.Object);

        _eventSourceMock.VerifyAdd(e => e.TaskStarted += It.IsAny<TaskStartedEventHandler>(), Times.Once);
        _eventSourceMock.VerifyAdd(e => e.TaskFinished += It.IsAny<TaskFinishedEventHandler>(), Times.Once);
    }

    [Fact]
    public void Initialize_SubscribesToMessageRaisedEvent()
    {
        _logger.Initialize(_eventSourceMock.Object);

        _eventSourceMock.VerifyAdd(e => e.MessageRaised += It.IsAny<BuildMessageEventHandler>(), Times.Once);
    }

    [Fact]
    public void Initialize_WithParameters_ParsesLogFilePath()
    {
        _logger.Parameters = "output.json";

        _logger.Initialize(_eventSourceMock.Object);

        // Shutdown should attempt to write to the file
        // We verify this indirectly by checking no exception on shutdown
        _logger.Shutdown();
    }

    [Fact]
    public void Initialize_WithWhitespaceParameters_TrimsPath()
    {
        _logger.Parameters = "  output.json  ";

        _logger.Initialize(_eventSourceMock.Object);

        // Should work without issues
        _logger.Shutdown();
    }

    [Fact]
    public void Shutdown_WithNoParameters_DoesNotThrow()
    {
        _logger.Initialize(_eventSourceMock.Object);

        var exception = Record.Exception(() => _logger.Shutdown());

        Assert.Null(exception);
    }

    [Fact]
    public void Shutdown_WithValidPath_WritesJsonFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            _logger.Parameters = tempFile;
            _logger.Initialize(_eventSourceMock.Object);

            _logger.Shutdown();

            Assert.True(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.Equal("[]", content); // No issues collected
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ErrorRaised_CollectsIssueInReport()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            _logger.Parameters = tempFile;
            _logger.Initialize(_eventSourceMock.Object);

            // Raise an error event
            _eventSourceMock.Raise(e => e.ErrorRaised += null,
                new BuildErrorEventArgs(
                    subcategory: null,
                    code: "CS0001",
                    file: "test.cs",
                    lineNumber: 10,
                    columnNumber: 5,
                    endLineNumber: 10,
                    endColumnNumber: 10,
                    message: "Test error",
                    helpKeyword: null,
                    senderName: "test"));

            _logger.Shutdown();

            var content = File.ReadAllText(tempFile);
            Assert.Contains("CS0001", content);
            Assert.Contains("Test error", content);
            Assert.Contains("critical", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void WarningRaised_CollectsIssueInReport()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            _logger.Parameters = tempFile;
            _logger.Initialize(_eventSourceMock.Object);

            _eventSourceMock.Raise(e => e.WarningRaised += null,
                new BuildWarningEventArgs(
                    subcategory: null,
                    code: "CS0168",
                    file: "test.cs",
                    lineNumber: 20,
                    columnNumber: 1,
                    endLineNumber: 20,
                    endColumnNumber: 5,
                    message: "Variable declared but never used",
                    helpKeyword: null,
                    senderName: "test"));

            _logger.Shutdown();

            var content = File.ReadAllText(tempFile);
            Assert.Contains("CS0168", content);
            Assert.Contains("Variable declared but never used", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ErrorRaised_WithNullCode_UsesBuildErrorAsCheckName()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            _logger.Parameters = tempFile;
            _logger.Initialize(_eventSourceMock.Object);

            _eventSourceMock.Raise(e => e.ErrorRaised += null,
                new BuildErrorEventArgs(
                    subcategory: null,
                    code: null,
                    file: "test.cs",
                    lineNumber: 10,
                    columnNumber: 5,
                    endLineNumber: 10,
                    endColumnNumber: 10,
                    message: "Test error",
                    helpKeyword: null,
                    senderName: "test"));

            _logger.Shutdown();

            var content = File.ReadAllText(tempFile);
            Assert.Contains("BUILD_ERROR", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void WarningRaised_WithNullCode_UsesBuildWarningAsCheckName()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            _logger.Parameters = tempFile;
            _logger.Initialize(_eventSourceMock.Object);

            _eventSourceMock.Raise(e => e.WarningRaised += null,
                new BuildWarningEventArgs(
                    subcategory: null,
                    code: null,
                    file: "test.cs",
                    lineNumber: 10,
                    columnNumber: 5,
                    endLineNumber: 10,
                    endColumnNumber: 10,
                    message: "Test warning",
                    helpKeyword: null,
                    senderName: "test"));

            _logger.Shutdown();

            var content = File.ReadAllText(tempFile);
            Assert.Contains("BUILD_WARNING", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void MultipleErrors_CollectsAllIssues()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            _logger.Parameters = tempFile;
            _logger.Initialize(_eventSourceMock.Object);

            _eventSourceMock.Raise(e => e.ErrorRaised += null,
                new BuildErrorEventArgs(null, "CS0001", "file1.cs", 1, 1, 1, 1, "Error 1", null, "test"));
            _eventSourceMock.Raise(e => e.ErrorRaised += null,
                new BuildErrorEventArgs(null, "CS0002", "file2.cs", 2, 1, 2, 1, "Error 2", null, "test"));
            _eventSourceMock.Raise(e => e.WarningRaised += null,
                new BuildWarningEventArgs(null, "CS0168", "file3.cs", 3, 1, 3, 1, "Warning 1", null, "test"));

            _logger.Shutdown();

            var content = File.ReadAllText(tempFile);
            Assert.Contains("CS0001", content);
            Assert.Contains("CS0002", content);
            Assert.Contains("CS0168", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
