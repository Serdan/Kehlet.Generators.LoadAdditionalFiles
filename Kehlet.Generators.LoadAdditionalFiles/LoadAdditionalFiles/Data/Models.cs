using System.Collections.Immutable;
using Kehlet.Generators.Attributes;
using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.Data;

internal record FileData(string Path, string Content);

internal record TargetOptions(
    string? RegexFilter,
    bool OmitFileExtension,
    string MemberNamePrefix,
    string MemberNameSuffix,
    MemberKind MemberKind
);

internal record TargetData(
    TypeData TypeData,
    ImmutableArray<TargetOptions> FileTargets
);

internal enum GeneratorErrors
{
    InvalidMemberKind,
    FileNotFound,
    MissingPartialKeyword
}

internal record GeneratorError(GeneratorErrors Error, string Details, Option<SafeLocation> Location) : IDiagnostic
{
    public Unit Report(SourceProductionContext context)
    {
        var descriptor = Error switch
        {
            GeneratorErrors.InvalidMemberKind => Diagnostics.InvalidMemberKind,
            GeneratorErrors.FileNotFound => Diagnostics.FileNotFound,
            GeneratorErrors.MissingPartialKeyword => Diagnostics.MissingPartialKeyword,
            _ => throw new ArgumentOutOfRangeException()
        };

        var diagnostic = Diagnostic.Create(descriptor, Location.ToObject()?.Location, Details);

        context.ReportDiagnostic(diagnostic);

        return unit;
    }
}
