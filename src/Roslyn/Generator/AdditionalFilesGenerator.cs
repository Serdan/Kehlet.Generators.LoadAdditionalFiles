using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

[Generator]
public partial class AdditionalFilesGenerator : IIncrementalGenerator
{
    private static readonly string attributeName = typeof(LoadAdditionalFilesAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var attributeStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.LoadAdditionalFilesAttribute.cs")!;
        var memberKindEnumStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.MemberKind.cs")!;
        var embeddedStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.EmbeddedAttribute.cs")!;

        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(attributeName, SourceText.From(attributeStream, Encoding.UTF8));
            ctx.AddSource(typeof(MemberKind).FullName!, SourceText.From(memberKindEnumStream, Encoding.UTF8));
            ctx.AddSource(typeof(EmbeddedAttribute).FullName!, SourceText.From(embeddedStream, Encoding.UTF8));
        });

        var textValues = context.AdditionalTextsProvider.Select(Parser.GetText).Choose();
        var typeValues = context.CreateTargetProvider(attributeName, SyntaxTarget.Type, Parser.Parse).Choose();

        var typeValuesWithTexts = typeValues.Combine(textValues.Collect().WithComparer(Equality<FileData>.ArrayComparer));

        context.RegisterSourceOutput(typeValuesWithTexts, GenerateCode);
    }

    internal static void GenerateCode(SourceProductionContext context, StaticContentTypeData data, ImmutableArray<FileData> texts)
    {
        var source = new Emitter(data, texts).Visit(data.ModuleDescription).UnsafeValue.ToString();
        context.AddSourceUTF8(data.FileName, source);
    }
}
