using Kehlet.Generators.Attributes;

namespace Kehlet.Generators.LoadAdditionalFiles.Sample;

[LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.txt$", PropertyNameSuffix = "_text")]
[LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.cs$", PropertyNameSuffix = "Source")]
public static partial class Test;
