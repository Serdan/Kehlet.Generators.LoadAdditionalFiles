using System.Collections.Immutable;
using Kehlet.Generators.Attributes;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;
using Kehlet.SourceGenerator.Source;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

public partial class AdditionalFilesGenerator
{
    internal static class Parser
    {
        private static StaticContentOptions CreateTarget(AttributeData attribute)
        {
            const string filter = nameof(LoadAdditionalFilesAttribute.RegexFilter);
            const string omitFileExt = nameof(LoadAdditionalFilesAttribute.OmitFileExtension);
            const string prefix = nameof(LoadAdditionalFilesAttribute.MemberNamePrefix);
            const string suffix = nameof(LoadAdditionalFilesAttribute.MemberNameSuffix);
            const string memberKind = nameof(LoadAdditionalFilesAttribute.MemberKind);

            var args = attribute.GetNamedArguments();

            return new(
                RegexFilter: args.GetValueAs<string>(filter).DefaultValue(null!),
                OmitFileExtension: args.GetValueAs<bool>(omitFileExt).DefaultValue(true),
                MemberNamePrefix: args.GetValueAs<string>(prefix).DefaultValue(""),
                MemberNameSuffix: args.GetValueAs<string>(suffix).DefaultValue(""),
                MemberKind: args.GetValueAs<MemberKind>(memberKind).DefaultValue(MemberKind.Field)
            );
        }

        public static Result<StaticContentTypeData, GeneratorError> Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            var syntax = (TypeDeclarationSyntax)context.TargetNode;

            if (syntax.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
            {
                return new GeneratorError(GeneratorErrors.MissingPartialKeyword, syntax.Identifier.Text);
            }

            var fileTargets = ImmutableArray.CreateBuilder<StaticContentOptions>();
            foreach (var attribute in context.Attributes)
            {
                var fileTarget = CreateTarget(attribute);
                if (EnumHelper.HasMember<MemberKind>((int)fileTarget.MemberKind) is false)
                {
                    return new GeneratorError(GeneratorErrors.InvalidMemberKind, syntax.Identifier.Text);
                }

                fileTargets.Add(fileTarget);
            }

            var symbol = (INamedTypeSymbol)context.TargetSymbol;

            return new StaticContentTypeData(TypeFullData.From(symbol, syntax), fileTargets.ToImmutable());
        }

        public static Result<FileData, GeneratorError> GetText(AdditionalText text, CancellationToken ct)
        {
            var path = text.Path;
            var content = text.GetText(ct)?.ToString();
            if (content is null)
            {
                return new GeneratorError(GeneratorErrors.FileNotFound, path);
            }

            return new FileData(path, content);
        }
    }
}
