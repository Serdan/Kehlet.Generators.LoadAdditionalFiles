using Kehlet.Generators.LoadAdditionalFiles.Common;
using Tests.Common;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Kehlet.Generators.LoadAdditionalFiles.Analyzer.LoadAdditionalFilesAnalyzer,
    Kehlet.Generators.LoadAdditionalFiles.CodeFixes.LoadAdditionalFilesCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier
>;

namespace LoadAdditionalFiles.Analyzer.Tests;

public class LoadAdditionalFilesCodeFixProviderTests
{
    [Fact]
    public async Task ClassWithMyCompanyTitle_ReplaceWithCommonKeyword()
    {
        var original = SR.GetClassWithAttribute(true);
        var fixedSource = SR.GetPartialClassWithAttribute(true);

        var expected = Verifier.Diagnostic(Diagnostics.MissingPartialKeyword)
                               .WithSpan(7, 18, 7, 29);

        await Verifier.VerifyCodeFixAsync(original, expected, fixedSource);
    }
}
