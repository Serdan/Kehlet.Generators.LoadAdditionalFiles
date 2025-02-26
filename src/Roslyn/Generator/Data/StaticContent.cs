using Kehlet.Generators.Attributes;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator.Data;

[LoadAdditionalFiles(MemberNameSuffix = "Source", MemberKind = MemberKind.Field, RegexFilter = @"\.cs$")]
internal static partial class StaticContent;
