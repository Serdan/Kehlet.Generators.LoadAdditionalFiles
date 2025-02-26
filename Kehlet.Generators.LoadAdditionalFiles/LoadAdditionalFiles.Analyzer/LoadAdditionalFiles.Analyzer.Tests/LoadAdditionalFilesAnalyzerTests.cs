using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    LoadAdditionalFiles.Analyzer.LoadAdditionalFilesAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace LoadAdditionalFiles.Analyzer.Tests;

public class LoadAdditionalFilesAnalyzerTests
{
    [Fact]
    public async Task ClassWithMyCompanyTitle_AlertDiagnostic()
    {
        var text =
            $$"""
            using System;
            using Kehlet.Generators.Attributes;

            [LoadAdditionalFiles]
            public class MyTestClass
            {
            }

            {{SR.GeneratorTypes}}
            """;


        var expected = Verifier.Diagnostic()
                               .WithSpan(5, 14, 5, 25);

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
}
