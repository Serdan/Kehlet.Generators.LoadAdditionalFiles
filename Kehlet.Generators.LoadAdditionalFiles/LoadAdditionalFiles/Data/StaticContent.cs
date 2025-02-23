﻿using Kehlet.Generators.Attributes;

namespace Kehlet.Generators.LoadAdditionalFiles.Data;

[LoadAdditionalFiles(MemberNameSuffix = "Source", MemberKind = MemberKind.Field, RegexFilter = @"\.cs$")]
internal static partial class StaticContent
{
    internal const string AttributeName = "LoadAdditionalFilesAttribute";
    internal const string AttributeFullName = $"Kehlet.Generators.Attributes.{AttributeName}";
}
