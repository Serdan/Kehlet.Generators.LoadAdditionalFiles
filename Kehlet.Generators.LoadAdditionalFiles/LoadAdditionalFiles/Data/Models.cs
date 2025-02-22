using System.Collections.Immutable;
using Kehlet.Generators.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.Generators.LoadAdditionalFiles.Data;

internal readonly record struct FileData(string Path, string Content);

internal readonly record struct FileTarget(
    string? RegexFilter,
    bool OmitFileExtension,
    string MemberNamePrefix,
    string MemberNameSuffix,
    MemberKind MemberKind
);

internal readonly record struct TypeTarget(
    string Name,
    string Declaration,
    string Namespace,
    ImmutableArray<FileTarget> FileTargets
);

internal enum TypeTargetErrors
{
    InvalidMemberKind,
    FileNotFound,
    MissingPartialKeyword
}

internal readonly record struct LocationData(string FilePath, TextSpan SourceSpan, LinePositionSpan LinePositionSpan)
{
    public Location ToLocation() => Location.Create(FilePath, SourceSpan, LinePositionSpan);

    public static Option<LocationData> FromLocation(Location? location)
    {
        if (location is null)
        {
            return None;
        }
        
        var filePath = location.SourceTree?.FilePath;
        if (filePath is null)
        {
            return None;
        }

        var sourceSpan = location.SourceSpan;
        var linePositionSpan = location.GetLineSpan().Span;

        return new LocationData(filePath, sourceSpan, linePositionSpan);
    }
}

internal readonly record struct TypeTargetError(TypeTargetErrors Errors, string Details, Option<LocationData> Location);
