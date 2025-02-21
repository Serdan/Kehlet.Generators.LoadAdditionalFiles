using Kehlet.Generators.Attributes;

namespace Kehlet.Generators.LoadAdditionalFiles.Sample;

[LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.txt$", MemberNameSuffix = "_text")]
[LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.cs$", MemberNameSuffix = "Source", MemberKind = MemberKind.Property)]
public static partial class Test;

[LoadAdditionalFiles]
public static partial class Test2;
