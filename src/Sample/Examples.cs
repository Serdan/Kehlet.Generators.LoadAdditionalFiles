using Kehlet.Generators.Attributes;

namespace Kehlet.Generators.LoadAdditionalFiles.Sample;

public partial class Container
{
    [LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.txt$", MemberNameSuffix = "_text")]
    [LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.cs$", MemberNameSuffix = "Source", MemberKind = MemberKind.Field)]
    public static partial class Test;
}
