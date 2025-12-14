using System.CommandLine;

var toolDir = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
var loggerDll = Path.Combine(toolDir, "DotNet.GitlabCodeQualityBuildLogger.dll");

var outputFileOption = new Option<string>("--output", "-o")
{
    Description = "Path to the output file",
    DefaultValueFactory = _ => string.Empty
};

var rootCommand = new RootCommand("Convenience wrapper to enable msbuild logger configuration to be semi-automatic");
rootCommand.Options
    .Add(outputFileOption);

rootCommand.SetAction((res) =>
{
    Console.Write($"DotNet.GitlabCodeQualityBuildLogger.CustomBuildLogger,{loggerDll}");
    if (res.GetValue(outputFileOption) != string.Empty)
    {
        Console.Write($";{res.GetValue(outputFileOption)}");   
    }
});

rootCommand
    .Parse(args)
    .Invoke();
