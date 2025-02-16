using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Kehlet.Generators.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.Generators.LoadAdditionalFiles;

using static StaticContent;

[Generator]
public class AdditionalFilesGenerator : IIncrementalGenerator
{
    private static Diagnostic InvalidMemberKindDiagnostic(string value, Location? location) =>
        Diagnostic.Create(
            new("LAF0001", "Invalid MemberKind", "The value given for MemberKind is not supported: {0}", "SourceGenerator", DiagnosticSeverity.Error, true),
            location,
            value
        );

    private static Diagnostic FileNotFoundDiagnostic(string fileName, Location? location) =>
        Diagnostic.Create(
            new("LAF0002", "Loading file failed", "File could not be loaded by the generator: {0}", "SourceGenerator", DiagnosticSeverity.Warning, true),
            location,
            fileName
        );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{AttributeName}.g.cs", SourceText.From(LoadAdditionalFilesAttributeSource, Encoding.UTF8));
            ctx.AddSource("MemberKind.g.cs", SourceText.From(MemberKindSource, Encoding.UTF8));
        });

        var texts = context.AdditionalTextsProvider.Collect();
        var provider = context.SyntaxProvider
                              .ForAttributeWithMetadataName(AttributeFullName, IsValidTarget, Transform)
                              .SelectMany((x, _) => x is null ? ImmutableArray<TypeTarget>.Empty : ImmutableArray.Create(x.Value))
                              .Combine(texts);

        context.RegisterSourceOutput(provider, (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
    }

    private static bool IsValidTarget(SyntaxNode node, CancellationToken _) =>
        node is TypeDeclarationSyntax type && type.Modifiers.Any(SyntaxKind.PartialKeyword);

    private readonly record struct FileTarget(
        string? RegexFilter,
        bool OmitFileExtension,
        string MemberNamePrefix,
        string MemberNameSuffix,
        MemberKind MemberKind
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

        builder.Append("partial ");

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
            var memberKind = MemberKind.Field;

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
                    case nameof(FileTarget.MemberNamePrefix):
                        prefix = namedArgument.Value.Value as string ?? "";
                        break;
                    case nameof(FileTarget.MemberNameSuffix):
                        suffix = namedArgument.Value.Value as string ?? "";
                        break;
                    case nameof(FileTarget.MemberKind):
                        memberKind = namedArgument.Value.Value is int value ? (MemberKind) value : MemberKind.Field;
                        break;
                }
            }

            fileTargets.Add(new(regex, omit, prefix, suffix, memberKind));
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
            EmitMembers(context, fileTarget, texts, builder, context.CancellationToken);
        }

        var ns = string.IsNullOrWhiteSpace(typeTarget.Namespace) ? "" : $"namespace {typeTarget.Namespace};";
        var members = builder.ToString();

        var source = string.Format(format, ns, typeTarget.Declaration, members);

        context.AddSource($"{typeTarget.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static Unit EmitField(string memberName, string quotes, string sourceText, StringBuilder builder)
    {
        const string memberDeclFormat = "    public static readonly string {0} =";
        const string emptyMemberFormat = $"""{memberDeclFormat} "";""";
        const string memberFormat =
            $$"""
            {{memberDeclFormat}}
            {1}
            {2}
            {1};

            """;

        if (string.IsNullOrEmpty(sourceText))
        {
            builder.AppendFormat(emptyMemberFormat, memberName);
            builder.AppendLine();
        }
        else
        {
            builder.AppendFormat(memberFormat, memberName, quotes, sourceText);
        }

        return default;
    }

    private static Unit EmitConstant(string memberName, string quotes, string sourceText, StringBuilder builder)
    {
        const string memberDeclFormat = "    public const string {0} =";
        const string emptyMemberFormat = $"""{memberDeclFormat} "";""";
        const string memberFormat =
            $$"""
            {{memberDeclFormat}}
            {1}
            {2}
            {1};

            """;

        if (string.IsNullOrEmpty(sourceText))
        {
            builder.AppendFormat(emptyMemberFormat, memberName);
            builder.AppendLine();
        }
        else
        {
            builder.AppendFormat(memberFormat, memberName, quotes, sourceText);
        }

        return default;
    }

    private static Unit EmitProperty(string memberName, string quotes, string sourceText, StringBuilder builder)
    {
        const string memberDeclFormat = "    public static string {0} =>";
        const string emptyMemberFormat = $"""{memberDeclFormat} "";""";
        const string memberFormat =
            $$"""
            {{memberDeclFormat}}
            {1}
            {2}
            {1};

            """;

        if (string.IsNullOrEmpty(sourceText))
        {
            builder.AppendFormat(emptyMemberFormat, memberName);
            builder.AppendLine();
        }
        else
        {
            builder.AppendFormat(memberFormat, memberName, quotes, sourceText);
        }

        return default;
    }

    private static void EmitMembers(SourceProductionContext context, FileTarget fileTarget, ImmutableArray<AdditionalText> texts, StringBuilder builder,
        CancellationToken ct)
    {
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
                context.Report(FileNotFoundDiagnostic(fileName, null));
                builder.AppendFormat("    // Failed to read file: {0}", fileName);
                continue;
            }

            var memberName = fileName.Replace('.', '_').Replace('`', '_');
            memberName = $"{fileTarget.MemberNamePrefix}{memberName}{fileTarget.MemberNameSuffix}";

            var quotes = GetRawStringQuotes(sourceText);

            _ = fileTarget.MemberKind switch
            {
                MemberKind.Field => EmitField(memberName, quotes, sourceText, builder),
                MemberKind.Constant => EmitConstant(memberName, quotes, sourceText, builder),
                MemberKind.Property => EmitProperty(memberName, quotes, sourceText, builder),
                var value => context.Report(InvalidMemberKindDiagnostic(value.ToString(), null))
            };
        }
    }

    private static string GetRawStringQuotes(string file)
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

        return new('"', Math.Max(3, count));
    }
}
