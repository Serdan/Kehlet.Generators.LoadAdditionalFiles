using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.Common;

public static class Diagnostics
{
    public static readonly DiagnosticDescriptor InvalidMemberKind =
        new("AKLAF0001", Resources.AKLAF0001_Title, Resources.AKLAF0001_MessageFormat, "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor MissingPartialKeyword =
        new("AKLAF0002", Resources.AKLAF0002_Title, Resources.AKLAF0002_MessageFormat, "Usage", DiagnosticSeverity.Warning, true);
}
