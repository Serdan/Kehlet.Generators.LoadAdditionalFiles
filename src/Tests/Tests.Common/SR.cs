namespace Tests.Common;

public static class SR
{
    public static readonly string GeneratorTypes =
        """

        namespace Kehlet.Generators.LoadAdditionalFiles
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
            internal sealed class LoadAdditionalFilesAttribute : Attribute
            {
                public string? RegexFilter { get; set; }
            
                public bool OmitFileExtension { get; set; } = true;
            
                public string MemberNamePrefix { get; set; } = "";
            
                public string MemberNameSuffix { get; set; } = "";
            
                public MemberKind MemberKind { get; set; } = MemberKind.Field;
            }
            
            internal enum MemberKind
            {
                Field,
                Constant,
                Property
            }
        }
        """;

    public static string GetClassWithAttribute(bool includeGeneratorTypes = false) =>
        $$"""
        using System;
        using Kehlet.Generators.LoadAdditionalFiles;

        namespace MyTestNamespace.SubNamespace
        {
            [LoadAdditionalFiles]
            public class MyTestClass;
        }

        {{(includeGeneratorTypes ? GeneratorTypes : "")}}
        """;

    public static string GetPartialClassWithAttribute(bool includeGeneratorTypes = false) =>
        $$"""
        using System;
        using Kehlet.Generators.LoadAdditionalFiles;

        namespace MyTestNamespace.SubNamespace
        {
            [LoadAdditionalFiles]
            public partial class MyTestClass;
        }

        {{(includeGeneratorTypes ? GeneratorTypes : "")}}
        """;
        
    
    public static readonly string PartialClassWithAttribute =
        $$"""
        using System;
        using Kehlet.Generators.LoadAdditionalFiles.StaticContent;

        namespace MyTestNamespace.SubNamespace
        {
            [LoadAdditionalFiles]
            public partial class MyTestClass;
        }

        {{GeneratorTypes}}
        """;
}
