using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DotNet.GitlabCodeQualityBuildLogger;

public class CustomBuildLogger : Logger
{
    private string? _logFilePath;
    private readonly List<CodeQualityIssue> _issues = [];

    public override void Initialize(IEventSource eventSource)
    {
        // Parse parameters - the Parameters property contains the string after the logger name
        // Example: /logger:CustomBuildLogger;logfile.json
        // Parameters will be "logfile.json"
        if (!string.IsNullOrEmpty(Parameters))
        {
            _logFilePath = Parameters.Trim();
        }

        eventSource.ProjectStarted += OnProjectStarted;
        eventSource.ProjectFinished += OnProjectFinished;
        eventSource.TargetStarted += OnTargetStarted;
        eventSource.TargetFinished += OnTargetFinished;
        eventSource.TaskStarted += OnTaskStarted;
        eventSource.TaskFinished += OnTaskFinished;
        eventSource.ErrorRaised += OnErrorRaised;
        eventSource.WarningRaised += OnWarningRaised;
        eventSource.MessageRaised += OnMessageRaised;
    }

    public override void Shutdown()
    {
        // Write the JSON report on shutdown
        if (!string.IsNullOrEmpty(_logFilePath))
        {
            try
            {
                var json = CodeQualityReport.ToJson(_issues);
                File.WriteAllText(_logFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write report to '{_logFilePath}': {ex.Message}");
            }
        }
    }

    private void OnProjectStarted(object sender, ProjectStartedEventArgs e)
    {
        if (IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            Console.WriteLine($"Project Started: {e.ProjectFile}");
        }
    }

    private void OnProjectFinished(object sender, ProjectFinishedEventArgs e)
    {
        if (IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            Console.WriteLine($"Project Finished: {e.ProjectFile} - Success: {e.Succeeded}");
        }
    }

    private void OnTargetStarted(object sender, TargetStartedEventArgs e)
    {
        if (IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            Console.WriteLine($"  Target Started: {e.TargetName}");
        }
    }

    private void OnTargetFinished(object sender, TargetFinishedEventArgs e)
    {
        if (IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            Console.WriteLine($"  Target Finished: {e.TargetName} - Success: {e.Succeeded}");
        }
    }

    private void OnTaskStarted(object sender, TaskStartedEventArgs e)
    {
        if (IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
        {
            Console.WriteLine($"    Task Started: {e.TaskName}");
        }
    }

    private void OnTaskFinished(object sender, TaskFinishedEventArgs e)
    {
        if (IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
        {
            Console.WriteLine($"    Task Finished: {e.TaskName} - Success: {e.Succeeded}");
        }
    }

    private void OnErrorRaised(object sender, BuildErrorEventArgs e)
    {
        var checkName = !string.IsNullOrEmpty(e.Code) ? e.Code : "BUILD_ERROR";
        var message = e.Message ?? "Unknown error";
        var file = e.File ?? "unknown";
        var line = e.LineNumber;

        _issues.Add(CodeQualityIssue.Create(checkName, message, file, line, "critical"));

        if (IsVerbosityAtLeast(LoggerVerbosity.Quiet))
        {
            Console.WriteLine($"ERROR: {file}({line},{e.ColumnNumber}): {e.Code}: {message}");
        }
    }

    private void OnWarningRaised(object sender, BuildWarningEventArgs e)
    {
        var checkName = !string.IsNullOrEmpty(e.Code) ? e.Code : "BUILD_WARNING";
        var message = e.Message ?? "Unknown warning";
        var file = e.File ?? "unknown";
        var line = e.LineNumber;

        // Map warning severity based on code prefixes
        var severity = GetWarningSeverity(checkName);
        _issues.Add(CodeQualityIssue.Create(checkName, message, file, line, severity));

        if (IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            Console.WriteLine($"WARNING: {file}({line},{e.ColumnNumber}): {e.Code}: {message}");
        }
    }

    private void OnMessageRaised(object sender, BuildMessageEventArgs e)
    {
        if (IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            Console.WriteLine($"MESSAGE: {e.Message}");
        }
    }

    private static string GetWarningSeverity(string code)
    {
        // CA = Code Analysis rules - generally minor to major
        // CS = C# compiler warnings
        // IDE = IDE suggestions - typically minor
        // SA = StyleCop - minor style issues

        if (code.StartsWith("CA", StringComparison.OrdinalIgnoreCase))
        {
            // Security and reliability rules are more severe
            if (code.StartsWith("CA2", StringComparison.OrdinalIgnoreCase) || // Security
                code.StartsWith("CA5", StringComparison.OrdinalIgnoreCase))   // Security
                return "major";
            return "minor";
        }

        if (code.StartsWith("CS", StringComparison.OrdinalIgnoreCase))
        {
            return "major"; // Compiler warnings are important
        }

        if (code.StartsWith("IDE", StringComparison.OrdinalIgnoreCase) ||
            code.StartsWith("SA", StringComparison.OrdinalIgnoreCase))
        {
            return "info";
        }

        return "minor";
    }
}
