using System.Collections.Immutable;
using Kehlet.Generators.Attributes;
using Kehlet.Generators.LoadAdditionalFiles.Data;
using Kehlet.SourceGenerator.Source;
using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles;

using static StaticContent;

[Generator]
public partial class AdditionalFilesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterType<LoadAdditionalFilesAttribute>(LoadAdditionalFilesAttributeSource);
        context.RegisterType<MemberKind>(MemberKindSource);

        var textValues = context.AdditionalTextsProvider.Select(Parser.GetText).Values();
        var typeValues = context.SyntaxForAttribute(AttributeFullName, SyntaxTarget.Type, Parser.Parse).Values();

        var typeValuesWithTexts = typeValues.Combine(textValues.Collect().WithComparer(Equality<FileData>.ArrayComparer));

        context.RegisterSourceOutput(typeValuesWithTexts, static (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
    }

    internal static void GenerateCode(SourceProductionContext context, TargetData targetData, ImmutableArray<FileData> texts)
    {
        var typeEmitter = new Emitter(targetData, texts);
        context.AddSourceUTF8(targetData.TypeData.Type.GetFileName(), new StandardEmitter().File(typeEmitter, targetData.TypeData));
    }
}
