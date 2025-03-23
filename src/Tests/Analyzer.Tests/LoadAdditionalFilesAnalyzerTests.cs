using Kehlet.Generators.LoadAdditionalFiles.Analyzer;
using Kehlet.Generators.LoadAdditionalFiles.Common;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Tests.Common;
using Xunit;

namespace LoadAdditionalFiles.Analyzer.Tests;

using Verifier = CSharpAnalyzerVerifier<LoadAdditionalFilesAnalyzer, DefaultVerifier>;

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
