namespace Kehlet.Generators.LoadAdditionalFiles.Sample;

public partial class Container
{
    [LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.txt$", MemberNameSuffix = "_text")]
    [LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.cs$", MemberNameSuffix = "Source")]
    public partial class Test
    {
        static void Method() 
        {
            var a = Test.AdditionalFilesGenerator_ParserSource;
        }
    }
}
