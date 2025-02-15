using System.Collections.Immutable;
using Kehlet.Generators.LoadAdditionalFiles.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Kehlet.Generators.LoadAdditionalFiles.Analyzer;

using static Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoadAdditionalFilesAnalyzer : DiagnosticAnalyzer
{
    private const string AttributeName = "Kehlet.Generators.LoadAdditionalFiles.LoadAdditionalFilesAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        MissingPartialKeyword,
        InvalidMemberKind
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            FindStaticContentContainer,
            ClassDeclaration,
            StructDeclaration,
            InterfaceDeclaration,
            RecordDeclaration,
            RecordStructDeclaration
        );
    }

    private static void FindStaticContentContainer(SyntaxNodeAnalysisContext context)
    {
        var typeDeclarationNode = (TypeDeclarationSyntax)context.Node;

        var attributes = typeDeclarationNode.GetAttributesWithName(context.SemanticModel, AttributeName);
        if (attributes.IsEmpty)
        {
            return;
        }

        FindMissingPartial(context, typeDeclarationNode);
        foreach (var attribute in attributes)
        {
            CheckMemberKindArgument(context, attribute);
        }
    }

    private static void FindMissingPartial(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclarationNode)
    {
        var hasPartial = typeDeclarationNode.Modifiers.Any(PartialKeyword);
        if (hasPartial)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(MissingPartialKeyword, typeDeclarationNode.Identifier.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    private static void CheckMemberKindArgument(SyntaxNodeAnalysisContext context, AttributeData attributeData)
    {
        var isValid = attributeData.ValidateNamedEnumArgument<MemberKind>(nameof(LoadAdditionalFilesAttribute.MemberKind));
        if (isValid)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(
            InvalidMemberKind,
            attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()
        );

        context.ReportDiagnostic(diagnostic);
    }
}
