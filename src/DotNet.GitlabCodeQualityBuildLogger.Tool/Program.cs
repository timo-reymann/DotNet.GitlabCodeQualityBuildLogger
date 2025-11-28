var toolDir = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
var loggerDll = Path.Combine(toolDir, "DotNet.GitlabCodeQualityBuildLogger.dll");
Console.WriteLine($"DotNet.GitlabCodeQualityBuildLogger.CustomBuildLogger,{loggerDll}");
