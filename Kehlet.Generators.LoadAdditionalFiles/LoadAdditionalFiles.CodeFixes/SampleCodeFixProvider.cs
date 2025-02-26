using System.Collections.Immutable;
using System.Composition;
using LoadAdditionalFiles.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LoadAdditionalFiles.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SampleCodeFixProvider)), Shared]
public sealed class SampleCodeFixProvider : CodeFixProvider
{
    // Specify the diagnostic IDs of analyzers that are expected to be linked.
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [Diagnostics.MissingPartialKeyword.Id];

    // If you don't need the 'fix all' behaviour, return null.
    public override FixAllProvider? GetFixAllProvider() => null;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnosticNode = root?.FindNode(diagnosticSpan);

        if (diagnosticNode is not ClassDeclarationSyntax declaration)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Diagnostics.MissingPartialKeyword.Title.ToString(),
                createChangedSolution: ct => AddPartialKeywordAsync(context.Document, declaration, ct),
                equivalenceKey: nameof(Diagnostics.MissingPartialKeyword)
            ),
            diagnostic
        );
    }

    private static async Task<Solution> AddPartialKeywordAsync(
        Document document,
        ClassDeclarationSyntax classDeclarationSyntax,
        CancellationToken cancellationToken)
    {
        var partial = SyntaxFactory.Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(SyntaxFactory.Space);
        var modifiers = classDeclarationSyntax.Modifiers.Add(partial);
        var fixedDeclaration = classDeclarationSyntax.WithModifiers(modifiers);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document.Project.Solution;
        }

        var newRoot = root.ReplaceNode(classDeclarationSyntax, fixedDeclaration);
        var newDoc = document.WithSyntaxRoot(newRoot);

        return newDoc.Project.Solution;
    }
}
