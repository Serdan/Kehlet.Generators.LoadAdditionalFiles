using System.Collections.Immutable;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

public partial class LoadAdditionalFilesGenerator
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
                MemberNamePrefix: args.GetValueAs<string>(prefix).DefaultValue("")!,
                MemberNameSuffix: args.GetValueAs<string>(suffix).DefaultValue("")!,
                MemberKind: args.GetEnumValue<MemberKind>(memberKind).DefaultValue(MemberKind.Field)
            );
        }

        public static Option<StaticContentTypeData> Parse(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            var targetNode = (TypeDeclarationSyntax) context.TargetNode;

            if (targetNode.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
            {
                return None;
            }

            var fileTargets = ImmutableArray.CreateBuilder<StaticContentOptions>();
            foreach (var attribute in context.Attributes)
            {
                fileTargets.Add(CreateTarget(attribute));
            }

            var moduleData = SyntaxHelper.GetTargetWithContext(targetNode);
            if (moduleData.IsNone)
            {
                return None;
            }

            var @namespace = NamespaceVisitor.GetFullNamespace(moduleData.UnsafeValue).Map(x => x + ".").DefaultValue("");
            var identifier = targetNode.Identifier.ValueText;
            var fileName = @namespace + identifier;
            if (targetNode.Arity > 0)
            {
                fileName += $"`{targetNode.Arity}";
            }

            fileName += ".g.cs";

            return new StaticContentTypeData(
                fileName,
                moduleData.UnsafeValue,
                fileTargets.ToCacheArray()
            );
        }

        public static Option<FileData> GetText(AdditionalText text, CancellationToken ct)
        {
            var path = text.Path;
            var content = text.GetText(ct)?.ToString();
            if (content is null)
            {
                return None;
            }

            return new FileData(path, content);
        }
    }

    internal class NamespaceVisitor : SyntaxDescriptionWalker
    {
        public Option<string> Namespace = None;

        public override Unit VisitNamespace(NamespaceDescription description)
        {
            if (description.IsFileScoped)
            {
                Namespace = description.Name;
                return unit;
            }

            Namespace = Namespace.Map(x => x + ".").DefaultValue("") + description.Name;

            return base.VisitNamespace(description);
        }

        public static Option<string> GetFullNamespace(ModuleDescription description)
        {
            var visitor = new NamespaceVisitor();
            visitor.Visit(description);
            return visitor.Namespace;
        }
    }
}
