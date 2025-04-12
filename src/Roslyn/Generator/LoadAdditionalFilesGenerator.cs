using System.Collections.Immutable;
using System.Reflection;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;
using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

[Generator]
public partial class LoadAdditionalFilesGenerator : IIncrementalGenerator
{
    private static readonly string attributeName = typeof(LoadAdditionalFilesAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            var assembly = Assembly.GetExecutingAssembly();

            var attributeStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.LoadAdditionalFilesAttribute.cs")!;
            var memberKindEnumStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.MemberKind.cs")!;
            var embeddedStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.EmbeddedAttribute.cs")!;

            ctx.AddSource<LoadAdditionalFilesAttribute>(attributeStream);
            ctx.AddSource<MemberKind>(memberKindEnumStream);
            ctx.AddSource<EmbeddedAttribute>(embeddedStream);
        });

        var textValues = context.AdditionalTextsProvider.Select(Parser.GetText).Choose();
        var typeValues = context.CreateTargetProvider(attributeName, SyntaxTarget.Type, Parser.Parse).Choose();

        var typeValuesWithTexts = typeValues.Combine(textValues.Collect().WithComparer(Equality<FileData>.ArrayComparer));

        context.RegisterSourceOutput(typeValuesWithTexts, GenerateCode);
    }

    internal static void GenerateCode(SourceProductionContext context, StaticContentTypeData data, ImmutableArray<FileData> texts)
    {
        var emitter = new FilesEmitter(data, texts);
        emitter.Visit(data.ModuleDescription);
        context.AddSource(data.FileName, emitter.ToString());
    }
}
