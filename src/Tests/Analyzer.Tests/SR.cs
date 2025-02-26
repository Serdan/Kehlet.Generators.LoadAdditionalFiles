namespace LoadAdditionalFiles.Analyzer.Tests;

public static class SR
{
    public static readonly string GeneratorTypes =
        """
        
        namespace Kehlet.Generators.Attributes
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
}
