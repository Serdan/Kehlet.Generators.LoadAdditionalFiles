using System.Collections.Immutable;
using Kehlet.Generators.Attributes;
using Microsoft.CodeAnalysis;

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

internal readonly record struct TypeTargetError(TypeTargetErrors Errors, string Details, Location? Location);