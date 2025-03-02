using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Kehlet.Generators.LoadAdditionalFiles.Common;
using Kehlet.Generators.LoadAdditionalFiles.StaticContent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Kehlet.Generators.LoadAdditionalFiles.Analyzer;

using static Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoadAdditionalFilesAnalyzer : DiagnosticAnalyzer
{
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

        var attributes = typeDeclarationNode.GetAttributesWithName(context.SemanticModel, MetaData.LoadAdditionalFilesAttributeFullName);
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
        var isValid = attributeData.VerifyNamedEnumArgument<MemberKind>(nameof(LoadAdditionalFilesAttribute.MemberKind));
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

internal static class Extensions
{
    private static readonly ConcurrentDictionary<string, (string, string)> Names = new();

    internal static (string simpleName, string simpleNameAttribute) GetSimpleName(string name)
    {
        Debug.Assert(name.Length > 0);

        if (Names.TryGetValue(name, out var result))
        {
            return result;
        }

        const char dot = '.';
        const string attribute = "Attribute";
        var attributeSpan = attribute.AsSpan();
        var nameSpan = name.AsSpan();
        var lastDotIndex = -1;
        for (var i = nameSpan.Length - 1; i >= 0; i--)
        {
            if (nameSpan[i] != dot)
            {
                continue;
            }

            lastDotIndex = i;
            break;
        }

        (string, string) tuple;
        if (nameSpan.EndsWith(attributeSpan, StringComparison.Ordinal))
        {
            var simpleNameAttribute = nameSpan[(lastDotIndex + 1)..];
            var simpleName = simpleNameAttribute[..^attributeSpan.Length];
            tuple = (simpleName.ToString(), simpleNameAttribute.ToString());
        }
        else
        {
            var simpleName = nameSpan[(lastDotIndex + 1)..].ToString();
            var simpleNameAttribute = simpleName + attribute;
            tuple = (simpleName, simpleNameAttribute);
        }

        Names.TryAdd(name, tuple);
        return tuple;
    }

    public static ImmutableArray<AttributeData> GetAttributesWithName(this TypeDeclarationSyntax syntax, SemanticModel semanticModel, string name)
    {
        var builder = ImmutableArray.CreateBuilder<AttributeData>();
        var (simpleName, simpleNameAttr) = GetSimpleName(name);

        foreach (var attributeList in syntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName != simpleName && attributeName != simpleNameAttr)
                {
                    continue;
                }

                var typeSymbol = semanticModel.GetDeclaredSymbol(syntax);
                if (typeSymbol is null)
                {
                    continue;
                }

                var attributeDatas = typeSymbol.GetAttributes();
                foreach (var attributeData in attributeDatas)
                {
                    if (attributeData.ApplicationSyntaxReference is null)
                    {
                        continue;
                    }

                    if (attributeData.ApplicationSyntaxReference.GetSyntax() == attribute)
                    {
                        builder.Add(attributeData);
                    }
                }
            }
        }

        return builder.ToImmutable();
    }
}
