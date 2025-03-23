using Kehlet.Generators.LoadAdditionalFiles.Analyzer;
using Kehlet.Generators.LoadAdditionalFiles.CodeFixes;
using Kehlet.Generators.LoadAdditionalFiles.Common;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Tests.Common;
using Xunit;

namespace LoadAdditionalFiles.Analyzer.Tests;

using Verifier = CSharpCodeFixVerifier<LoadAdditionalFilesAnalyzer, LoadAdditionalFilesCodeFixProvider, DefaultVerifier>;

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
