using System.Collections.Immutable;
using Kehlet.Generators.Attributes;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;
using Kehlet.Generators.LoadAdditionalFiles.StaticContent;
using Kehlet.SourceGenerator.Source;
using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

using static Data.StaticContent;

[Generator]
public partial class AdditionalFilesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterType(MetaData.EmbeddedAttributeFullName, EmbeddedAttributeSource);
        context.RegisterType<LoadAdditionalFilesAttribute>(LoadAdditionalFilesAttributeSource);
        context.RegisterType<MemberKind>(MemberKindSource);

        var textValues = context.AdditionalTextsProvider.Select(Parser.GetText).Values();
        var typeValues = context.SyntaxForAttribute(MetaData.LoadAdditionalFilesAttributeFullName, SyntaxTarget.Type, Parser.Parse).Values();

        var typeValuesWithTexts = typeValues.Combine(textValues.Collect().WithComparer(Equality<FileData>.ArrayComparer));

        context.RegisterSourceOutput(typeValuesWithTexts, static (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
    }

    internal static void GenerateCode(SourceProductionContext context, StaticContentTypeData staticContentTypeData, ImmutableArray<FileData> texts)
    {
        var typeEmitter = new Emitter(staticContentTypeData, texts);
        context.AddSourceUTF8(staticContentTypeData.TypeData.Type.GetFileName(), new StandardEmitter().File(typeEmitter, staticContentTypeData.TypeData));
    }
}
