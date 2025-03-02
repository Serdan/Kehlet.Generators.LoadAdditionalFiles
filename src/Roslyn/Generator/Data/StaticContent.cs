using Kehlet.Generators.LoadAdditionalFiles.StaticContent;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator.Data;

[LoadAdditionalFiles(MemberNameSuffix = "Source", MemberKind = MemberKind.Field, RegexFilter = @"\.cs$")]
internal static partial class StaticContent;
