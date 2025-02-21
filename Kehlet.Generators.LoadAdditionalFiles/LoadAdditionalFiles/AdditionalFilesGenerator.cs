using System.Collections.Immutable;
using System.Text;
using Kehlet.Generators.Attributes;
using Kehlet.Generators.LoadAdditionalFiles.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.Generators.LoadAdditionalFiles;

using static StaticContent;
using static Diagnostics;

[Generator]
public partial class AdditionalFilesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{nameof(LoadAdditionalFilesAttribute)}.g.cs", SourceText.From(LoadAdditionalFilesAttributeSource, Encoding.UTF8));
            ctx.AddSource($"{nameof(MemberKind)}.g.cs", SourceText.From(MemberKindSource, Encoding.UTF8));
        });

        var (textValues, textErrors) = context.AdditionalTextsProvider.Select(Parser.GetText).Partition();
        var (typeValues, typeErrors) = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeFullName, IsValidTarget, Parser.Parse).Partition();

        var typeValuesWithTexts = typeValues.Combine(textValues.Collect());

        context.RegisterSourceOutput(typeValuesWithTexts, static (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
        context.RegisterSourceOutput(typeErrors, ReportDiagnostics);
        context.RegisterSourceOutput(textErrors, ReportDiagnostics);
    }

    internal static bool IsValidTarget(SyntaxNode node, CancellationToken _) => node is TypeDeclarationSyntax;

    internal static void ReportDiagnostics(SourceProductionContext context, TypeTargetError error)
    {
        var diagnostic = error.Errors switch
        {
            TypeTargetErrors.InvalidMemberKind => InvalidMemberKindDiagnostic(error.Details, error.Location),
            TypeTargetErrors.FileNotFound => FileNotFoundDiagnostic(error.Details, error.Location),
            TypeTargetErrors.MissingPartialKeyword => MissingPartialKeywordDiagnostic(error.Details, error.Location),
            _ => throw new InvalidOperationException()
        };

        context.ReportDiagnostic(diagnostic);
    }

    internal static void GenerateCode(SourceProductionContext context, TypeTarget typeTarget, ImmutableArray<FileData> texts)
    {
        const string format =
            """
            #nullable enable

            {0}

            {1}
            {{
            {2}
            }}
            """;

        var builder = new StringBuilder();

        foreach (var fileTarget in typeTarget.FileTargets)
        {
            Emitter.EmitMembers(fileTarget, texts, builder);
        }

        var ns = string.IsNullOrWhiteSpace(typeTarget.Namespace) ? "" : $"namespace {typeTarget.Namespace};";
        var members = builder.ToString();

        var source = string.Format(format, ns, typeTarget.Declaration, members);

        context.AddSource($"{typeTarget.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
    }
}
