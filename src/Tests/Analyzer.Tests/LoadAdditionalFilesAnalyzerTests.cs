using Kehlet.Generators.LoadAdditionalFiles.Common;
using Tests.Common;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Kehlet.Generators.LoadAdditionalFiles.Analyzer.LoadAdditionalFilesAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace LoadAdditionalFiles.Analyzer.Tests;

public class LoadAdditionalFilesAnalyzerTests
{
    [Fact]
    public async Task ClassWithMyCompanyTitle_AlertDiagnostic()
    {
        var text = SR.GetClassWithAttribute(true);

        var expected = Verifier.Diagnostic(Diagnostics.MissingPartialKeyword)
                               .WithSpan(7, 18, 7, 29);

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }
}
