using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;

namespace DotNet.GitlabCodeQualityBuildLogger.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }
}
