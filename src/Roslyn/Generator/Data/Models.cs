using System.Collections.Immutable;
using Kehlet.Generators.Attributes;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator.Data;

internal record FileData(string Path, string Content);

internal record StaticContentOptions(
    string? RegexFilter,
    bool OmitFileExtension,
    string MemberNamePrefix,
    string MemberNameSuffix,
    MemberKind MemberKind
);

internal record StaticContentTypeData(
    TypeFullData TypeData,
    ImmutableArray<StaticContentOptions> Options
);

internal enum GeneratorErrors
{
    InvalidMemberKind,
    FileNotFound,
    MissingPartialKeyword
}

internal record GeneratorError(GeneratorErrors Error, string Details);
