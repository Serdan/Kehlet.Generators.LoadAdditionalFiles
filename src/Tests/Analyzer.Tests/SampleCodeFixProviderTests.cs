using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Kehlet.Generators.LoadAdditionalFiles.Analyzer.LoadAdditionalFilesAnalyzer,
    Kehlet.Generators.LoadAdditionalFiles.CodeFixes.SampleCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace LoadAdditionalFiles.Analyzer.Tests;

public class SampleCodeFixProviderTests
{
    [Fact]
    public async Task ClassWithMyCompanyTitle_ReplaceWithCommonKeyword()
    {
        var original =
            $$"""
            using System;
            using Kehlet.Generators.Attributes;

            [LoadAdditionalFiles.Generator]
            public class MyTestClass
            {
            }

            {{SR.GeneratorTypes}}
            """;

        var fixedSource =
            $$"""
            using System;
            using Kehlet.Generators.Attributes;

            [LoadAdditionalFiles.Generator]
            public partial class MyTestClass
            {
            }

            {{SR.GeneratorTypes}}
            """;

        var expected = Verifier.Diagnostic()
                               .WithSpan(5, 14, 5, 25);

        await Verifier.VerifyCodeFixAsync(original, expected, fixedSource);
    }
}
