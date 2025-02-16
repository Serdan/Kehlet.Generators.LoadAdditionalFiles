using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.Generators.LoadAdditionalFiles;

[Generator]
public class AdditionalFilesGenerator : IIncrementalGenerator
{
    private const string AttributeName = "LoadAdditionalFilesAttribute";
    private const string AttributeFqn = $"Kehlet.Generators.Attributes.{AttributeName}";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{AttributeName}.g.cs",
            SourceText.From(StaticTypes.LoadAdditionalFilesAttributeSource, Encoding.UTF8))
        );

        var texts = context.AdditionalTextsProvider.Collect();
        var provider = context.SyntaxProvider
                              .ForAttributeWithMetadataName(AttributeFqn, IsValidTarget, Transform)
                              .SelectMany((x, _) => x is null ? ImmutableArray<TypeTarget>.Empty : ImmutableArray.Create(x.Value))
                              .Combine(texts);

        context.RegisterSourceOutput(provider, (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
    }

    private static bool IsValidTarget(SyntaxNode node, CancellationToken _) =>
        node is TypeDeclarationSyntax type && type.Modifiers.Any(SyntaxKind.PartialKeyword);

    private readonly record struct FileTarget(
        string? RegexFilter,
        bool OmitFileExtension,
        string PropertyNamePrefix,
        string PropertyNameSuffix
    );

    private readonly record struct TypeTarget(
        string Name,
        string Declaration,
        string Namespace,
        ImmutableArray<FileTarget> FileTargets
    );

    private static TypeTarget? Transform(GeneratorAttributeSyntaxContext context, CancellationToken token)
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

        var fileTargets = ImmutableArray.CreateBuilder<FileTarget>();
        foreach (var attribute in context.Attributes)
        {
            string? regex = null;
            var omit = true;
            var prefix = "";
            var suffix = "";

            foreach (var namedArgument in attribute.NamedArguments)
            {
                switch (namedArgument.Key)
                {
                    case nameof(FileTarget.RegexFilter):
                        regex = namedArgument.Value.Value as string;
                        break;
                    case nameof(FileTarget.OmitFileExtension):
                        omit = namedArgument.Value.Value as bool? ?? true;
                        break;
                    case nameof(FileTarget.PropertyNamePrefix):
                        prefix = namedArgument.Value.Value as string ?? "";
                        break;
                    case nameof(FileTarget.PropertyNameSuffix):
                        suffix = namedArgument.Value.Value as string ?? "";
                        break;
                }
            }

            fileTargets.Add(new(regex, omit, prefix, suffix));
        }

        return new(syntax.Identifier.Text, builder.ToString(), ns, fileTargets.ToImmutable());
    }

    private static void GenerateCode(SourceProductionContext context, TypeTarget typeTarget, ImmutableArray<AdditionalText> texts)
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
            EmitMembers(fileTarget, texts, builder, context.CancellationToken);
        }

        var ns = string.IsNullOrWhiteSpace(typeTarget.Namespace) ? "" : $"namespace {typeTarget.Namespace};";
        var members = builder.ToString();

        var source = string.Format(format, ns, typeTarget.Declaration, members);

        context.AddSource($"{typeTarget.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static void EmitMembers(FileTarget fileTarget, ImmutableArray<AdditionalText> texts, StringBuilder builder, CancellationToken ct)
    {
        const string emptyMemberFormat = """    public const string {0} = "";""";
        const string memberFormat =
            """
                public const string {0} =
            {1}
            {2}
            {1};

            """;

        foreach (var text in texts)
        {
            if (fileTarget.RegexFilter is { Length: > 0 } pattern && Regex.IsMatch(text.Path, pattern, RegexOptions.Compiled) is false)
            {
                continue;
            }

            var fileName = Path.GetFileName(text.Path);
            if (fileTarget.OmitFileExtension)
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }

            var sourceText = text.GetText(ct)?.ToString();
            if (sourceText is null)
            {
                builder.AppendFormat("    // Failed to read file: {0}", fileName);
                continue;
            }

            var name = fileName.Replace('.', '_').Replace('`', '_');
            name = $"{fileTarget.PropertyNamePrefix}{name}{fileTarget.PropertyNameSuffix}";

            if (string.IsNullOrEmpty(sourceText))
            {
                builder.AppendFormat("    // File is empty: {0}", fileName);
                builder.AppendLine();
                builder.AppendFormat(emptyMemberFormat, name);
                builder.AppendLine();
                continue;
            }

            var count = Math.Max(Count(sourceText), 3);
            builder.AppendFormat(memberFormat, name, new string('"', count), text.GetText(ct));
        }
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
