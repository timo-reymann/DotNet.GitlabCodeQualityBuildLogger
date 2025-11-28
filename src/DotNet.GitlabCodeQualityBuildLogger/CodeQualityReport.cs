using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.GitlabCodeQualityBuildLogger;

public sealed class CodeQualityIssue
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("check_name")]
    public string CheckName { get; set; } = string.Empty;

    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public CodeQualityLocation Location { get; set; } = new();

    public static CodeQualityIssue Create(
        string checkName,
        string message,
        string filePath,
        int line,
        string severity)
    {
        var description = $"{checkName}: {message}";
        var fingerprint = GenerateFingerprint(filePath, line, checkName, message);

        return new CodeQualityIssue
        {
            Description = description,
            CheckName = checkName,
            Fingerprint = fingerprint,
            Severity = severity,
            Location = new CodeQualityLocation
            {
                Path = NormalizePath(filePath),
                Lines = new CodeQualityLines { Begin = line > 0 ? line : 1 }
            }
        };
    }

    private static string GenerateFingerprint(string file, int line, string code, string message)
    {
        var content = $"{file}:{line}:{code}:{message}";
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
        return BitConverter.ToString(hashBytes).Replace("-", "");
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "unknown";

        // Use CI_PROJECT_DIR if set, otherwise fall back to current working directory
        var rootDir = Environment.GetEnvironmentVariable("CI_PROJECT_DIR")
                      ?? Environment.CurrentDirectory;

        var fullPath = Path.GetFullPath(path);
        var relativePath = Path.GetRelativePath(rootDir, fullPath);

        // Convert to forward slashes for consistency
        return relativePath.Replace('\\', '/');
    }
}

public sealed class CodeQualityLocation
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("lines")]
    public CodeQualityLines Lines { get; set; } = new();
}

public sealed class CodeQualityLines
{
    [JsonPropertyName("begin")]
    public int Begin { get; set; }
}

public static class CodeQualityReport
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static string ToJson(IEnumerable<CodeQualityIssue> issues)
    {
        return JsonSerializer.Serialize(issues, JsonOptions);
    }
}
