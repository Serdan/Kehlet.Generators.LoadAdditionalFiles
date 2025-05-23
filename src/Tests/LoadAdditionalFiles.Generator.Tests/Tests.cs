﻿using Kehlet.Generators.LoadAdditionalFiles.Generator;
using Microsoft.CodeAnalysis;
using Tests.Common;

namespace LoadAdditionalFiles.Generator.Tests;

public class Tests
{
    [Fact]
    public Task VerifyVerify() => VerifyChecks.Run();

    [Fact]
    public Task GenerateEmptyPartialClass()
    {
        var generator = new LoadAdditionalFilesGenerator();
        var comp = CompilationFactory.PartialClassWithAttribute();
        var driver = GeneratorDriverFactory.WithNoFiles(generator).RunGeneratorsAndUpdateCompilation(comp, out var resultComp, out _);

        var result = driver.GetRunResult().Results.Single();
        Assert.Empty(resultComp.GetDiagnostics().Where(x => x.Severity is not (DiagnosticSeverity.Info or DiagnosticSeverity.Hidden)));
        Assert.Equal(4, result.GeneratedSources.Length);
        return Verify(result.GeneratedSources.Last().SourceText.ToString());
    }

    [Fact]
    public Task GenerateOneMemberDefaultSettings()
    {
        var generator = new LoadAdditionalFilesGenerator();
        var comp = CompilationFactory.PartialClassWithAttribute();
        var driver = GeneratorDriverFactory.WithOneFile(generator).RunGeneratorsAndUpdateCompilation(comp, out var resultComp, out _);

        var result = driver.GetRunResult().Results.Single();
        Assert.Empty(resultComp.GetDiagnostics().Where(x => x.Severity is not (DiagnosticSeverity.Info or DiagnosticSeverity.Hidden)));
        Assert.Equal(4, result.GeneratedSources.Length);
        return Verify(result.GeneratedSources.Last().SourceText.ToString());
    }
}
