#nullable enable

using System;

namespace Kehlet.Generators.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class LoadAdditionalFilesAttribute : Attribute
{
    public string? RegexFilter { get; set; }
    public bool OmitFileExtension { get; set; } = true;
    public string PropertyNamePrefix { get; set; } = "";
    public string PropertyNameSuffix { get; set; } = "";
}
