namespace Kehlet.Generators.LoadAdditionalFiles.Sample;

public partial class Container
{
    [LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.txt$", MemberNameSuffix = "_text")]
    [LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.cs$", MemberNameSuffix = "Source", MemberKind = MemberKind.Property)]
    public partial record class Test<T, U>;

    public partial class Container2
    {
        [LoadAdditionalFiles(OmitFileExtension = true, RegexFilter = @"\.cs$", MemberNameSuffix = "Source", MemberKind = MemberKind.Constant)]
        public partial struct Test2;
    }
}
