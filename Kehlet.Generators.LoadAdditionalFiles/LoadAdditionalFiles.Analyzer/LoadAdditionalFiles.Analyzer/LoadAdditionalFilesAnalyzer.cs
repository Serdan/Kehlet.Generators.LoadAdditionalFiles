using System.Collections.Immutable;
using System.Linq;
using LoadAdditionalFiles.StaticContent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static LoadAdditionalFiles.Common.Diagnostics;

namespace LoadAdditionalFiles.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoadAdditionalFilesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [MissingPartialKeyword];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationNode)
        {
            return;
        }

        var attributes = context.ContainingSymbol?.GetAttributes().Select(x => x.AttributeClass.FullMetadataName());
        if (attributes is null)
        {
            return;
        }
        
        var hasAttribute = attributes.Any(name => name == MetaData.LoadAdditionalFilesAttributeFullName);
        if (!hasAttribute)
        {
            return;
        }

        var hasPartial = classDeclarationNode.Modifiers.Any(SyntaxKind.PartialKeyword);
        if (hasPartial)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(MissingPartialKeyword, classDeclarationNode.Identifier.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    // public static readonly DiagnosticDescriptor MissingPartialKeyword =
    //     new("AKLAF0003", Resources.AKLAF0003_Title, Resources.AKLAF0003_MessageFormat, "Usage", DiagnosticSeverity.Warning, true);
}

internal static class Extensions
{
    public static string? FullMetadataName(this INamedTypeSymbol? symbol) =>
        symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
}
