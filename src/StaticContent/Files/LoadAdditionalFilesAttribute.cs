#nullable enable

using System;
using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.StaticContent;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[Embedded]
internal sealed class LoadAdditionalFilesAttribute : Attribute
{
    public string? RegexFilter { get; set; }

    public bool OmitFileExtension { get; set; } = true;

    public string MemberNamePrefix { get; set; } = "";

    public string MemberNameSuffix { get; set; } = "";

    public MemberKind MemberKind { get; set; } = MemberKind.Field;
}
