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
    string FileName,
    ModuleDescription ModuleDescription,
    CacheArray<StaticContentOptions> Options
);
