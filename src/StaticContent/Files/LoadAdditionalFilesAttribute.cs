#nullable enable

using System;
using Microsoft.CodeAnalysis;
using static System.AttributeTargets;

namespace Kehlet.Generators.LoadAdditionalFiles;

[Embedded]
[AttributeUsage(Class | Struct | Interface, AllowMultiple = true)]
internal sealed class LoadAdditionalFilesAttribute : Attribute
{
    public string? RegexFilter { get; set; }

    public bool OmitFileExtension { get; set; } = true;

    public string MemberNamePrefix { get; set; } = "";

    public string MemberNameSuffix { get; set; } = "";

    public MemberKind MemberKind { get; set; } = MemberKind.Field;
}
