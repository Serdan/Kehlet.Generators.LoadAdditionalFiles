using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.Data;

internal static class Diagnostics
{
    private static readonly DiagnosticDescriptor invalidMemberKind =
        new("AKLAF0001", Resources.AKLAF0001_Title, Resources.AKLAF0001_MessageFormat, "Usage", DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor fileNotFound =
        new("AKLAF0002", Resources.AKLAF0002_Title, Resources.AKLAF0002_MessageFormat, "Usage", DiagnosticSeverity.Warning, true);

    private static readonly DiagnosticDescriptor missingPartialKeyword =
        new("AKLAF0003", Resources.AKLAF0003_Title, Resources.AKLAF0003_MessageFormat, "Usage", DiagnosticSeverity.Warning, true);

    public static Diagnostic InvalidMemberKindDiagnostic(string value, Location? location) =>
        Diagnostic.Create(invalidMemberKind, location, value);

    public static Diagnostic FileNotFoundDiagnostic(string fileName, Location? location) =>
        Diagnostic.Create(fileNotFound, location, fileName);

    public static Diagnostic MissingPartialKeywordDiagnostic(string typeName, Location? location) =>
        Diagnostic.Create(missingPartialKeyword, location, typeName);
}
