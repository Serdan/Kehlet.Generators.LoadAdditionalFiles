using System.Collections.Immutable;
using System.Text;
using Kehlet.Generators.Attributes;
using Kehlet.Generators.LoadAdditionalFiles.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.Generators.LoadAdditionalFiles;

public partial class AdditionalFilesGenerator
{
    internal static class Parser
    {
        private static string BuildDeclaration(TypeDeclarationSyntax syntax)
        {
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
            return builder.ToString();
        }

        private static FileTarget CreateTarget(AttributeData attribute)
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
                        memberKind = namedArgument.Value.Value is int value ? (MemberKind)value : MemberKind.Field;
                        break;
                }
            }

            return new(regex, omit, prefix, suffix, memberKind);
        }

        public static Result<TypeTarget, TypeTargetError> Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            var syntax = (TypeDeclarationSyntax)context.TargetNode;

            if (syntax.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
            {
                return new TypeTargetError(TypeTargetErrors.MissingPartialKeyword, syntax.Identifier.Text, syntax.Identifier.GetLocation());
            }

            var fileTargets = ImmutableArray.CreateBuilder<FileTarget>();
            foreach (var attribute in context.Attributes)
            {
                var fileTarget = CreateTarget(attribute);
                if (EnumHelper.HasMember<MemberKind>((int)fileTarget.MemberKind) is false)
                {
                    return Error(new TypeTargetError(TypeTargetErrors.InvalidMemberKind, syntax.Identifier.Text,
                        attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
                }


                fileTargets.Add(fileTarget);
            }

            var declaration = BuildDeclaration(syntax);
            var ns = ((INamedTypeSymbol)context.TargetSymbol).GetContainingNamespace();
            
            return new TypeTarget(syntax.Identifier.Text, declaration, ns, fileTargets.ToImmutable());
        }

        public static Result<FileData, TypeTargetError> GetText(AdditionalText text, CancellationToken ct)
        {
            var path = text.Path;
            var content = text.GetText(ct)?.ToString();
            if (content is null)
            {
                return Error(new TypeTargetError(TypeTargetErrors.FileNotFound, path, null));
            }

            return Ok(new FileData(path, content));
        }
    }
}