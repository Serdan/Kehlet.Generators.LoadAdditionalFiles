using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles;

internal static class Extensions
{
    public static Unit Report(this SourceProductionContext context, Diagnostic diagnostic)
    {
        context.ReportDiagnostic(diagnostic);
        return default;
    }
}

public readonly record struct Unit;
