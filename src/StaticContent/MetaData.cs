using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.StaticContent;

internal static class MetaData
{
    public static readonly string EmbeddedAttributeFullName = typeof(EmbeddedAttribute).FullName;
    public static readonly string LoadAdditionalFilesAttributeFullName = typeof(LoadAdditionalFilesAttribute).FullName;
    public static readonly string MemberKindFullName = typeof(MemberKind).FullName;
}
