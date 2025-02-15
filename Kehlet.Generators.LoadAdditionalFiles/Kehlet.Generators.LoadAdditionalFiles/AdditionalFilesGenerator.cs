using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.Generators.LoadAdditionalFiles;

[Generator]
public class AdditionalFilesGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Kehlet.Generators.Attributes";
    private const string AttributeName = "LoadAdditionalFilesAttribute";
    private const string AttributeFqn = $"{AttributeNamespace}.{AttributeName}";

    private const string AttributeSourceCode =
        // language=cs
        $"""
        #nullable enable

        using System;

        namespace {AttributeNamespace};

        [AttributeUsage(AttributeTargets.Class)]
        public sealed class {AttributeName} : Attribute;

        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{AttributeName}.g.cs",
            SourceText.From(AttributeSourceCode, Encoding.UTF8)));

        var texts = context.AdditionalTextsProvider.Collect();
        var provider = context.SyntaxProvider
                              .ForAttributeWithMetadataName(AttributeFqn, IsValidTarget, Transform)
                              .SelectMany((x, _) => x is null ? ImmutableArray<Target>.Empty : [x.Value])
                              .Combine(texts);

        context.RegisterSourceOutput(provider, (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
    }

    private static bool IsValidTarget(SyntaxNode node, CancellationToken _) =>
        node is TypeDeclarationSyntax type && type.Modifiers.Any(SyntaxKind.PartialKeyword);

    private readonly record struct Target(string Name, string Declaration, string Namespace);

    private static Target? Transform(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (context.TargetNode is not TypeDeclarationSyntax syntax)
        {
            return null;
        }

        var builder = new StringBuilder();

        if (syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            builder.Append("static ");
        }

        if (syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            builder.Append("partial ");
        }

        if (syntax is RecordDeclarationSyntax record)
        {
            builder.Append("record ");
            var keyword = record.ClassOrStructKeyword.Text;
            if (string.IsNullOrWhiteSpace(keyword) is false)
            {
                builder.AppendFormat("{0} ", keyword);
            }
        }
        else
        {
            builder.AppendFormat("{0} ", syntax.Keyword.Text);
        }

        builder.Append(syntax.Identifier.Text);

        var ns = (context.TargetSymbol as INamedTypeSymbol)?.ContainingNamespace.ToDisplayString() ?? "";

        return new(syntax.Identifier.Text, builder.ToString(), ns);
    }

    private static void GenerateCode(SourceProductionContext context, Target target, ImmutableArray<AdditionalText> texts)
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

        const string emptyMemberFormat = """    public const string {0} = "";""";
        const string memberFormat =
            """
                public const string {0} =
            {1}
            {2}
            {1};

            """;

        var builder = new StringBuilder();

        foreach (var text in texts)
        {
            var fileName = Path.GetFileName(text.Path);

            var sourceText = text.GetText(context.CancellationToken)?.ToString();
            if (sourceText is null)
            {
                builder.AppendFormat("    // Failed to read file: {0}", fileName);
                continue;
            }

            var name = fileName.Replace('.', '_').Replace('`', '_');
            if (string.IsNullOrEmpty(sourceText))
            {
                builder.AppendFormat("    // File is empty: {0}", fileName);
                builder.AppendLine();
                builder.AppendFormat(emptyMemberFormat, name);
                builder.AppendLine();
                continue;
            }

            var count = Math.Max(Count(sourceText), 3);
            builder.AppendFormat(memberFormat, name, new string('"', count), text.GetText(context.CancellationToken));
        }

        var ns = string.IsNullOrWhiteSpace(target.Namespace) ? "" : $"namespace {target.Namespace};";
        var members = builder.ToString();

        var source = string.Format(format, ns, target.Declaration, members);

        context.AddSource($"{target.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static int Count(string file)
    {
        var count = 0;
        var currentCount = 0;

        foreach (var character in file)
        {
            if (character == '"')
            {
                currentCount++;
            }
            else
            {
                currentCount = 1;
            }

            if (currentCount > count)
            {
                count = currentCount;
            }
        }

        return count;
    }
}
