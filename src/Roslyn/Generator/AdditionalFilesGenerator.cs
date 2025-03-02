using System.Collections.Immutable;
using System.Reflection;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;
using Kehlet.Generators.LoadAdditionalFiles.StaticContent;
using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

[Generator]
public partial class AdditionalFilesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        var attributeStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.LoadAdditionalFilesAttribute.cs")!;
        var memberKindEnumStream = assembly.GetManifestResourceStream("Kehlet.Generators.LoadAdditionalFiles.Generator.MemberKind.cs")!;
        
        var attributeSource = new StreamReader(attributeStream).ReadToEnd();
        var memberKindEnumSource = new StreamReader(memberKindEnumStream).ReadToEnd();
        
        context.RegisterType<LoadAdditionalFilesAttribute>(attributeSource);
        context.RegisterType<MemberKind>(memberKindEnumSource);

        var textValues = context.AdditionalTextsProvider.Select(Parser.GetText).Values();
        var typeValues = context.SyntaxForAttribute(MetaData.LoadAdditionalFilesAttributeFullName, SyntaxTarget.Type, Parser.Parse).Values();

        var typeValuesWithTexts  = typeValues.Combine(textValues.Collect().WithComparer(Equality<FileData>.ArrayComparer));

        context.RegisterSourceOutput(typeValuesWithTexts, static (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
    }

    internal static void GenerateCode(SourceProductionContext context, StaticContentTypeData staticContentTypeData, ImmutableArray<FileData> texts)
    {
        var typeEmitter = new Emitter(staticContentTypeData, texts);
        context.AddSourceUTF8(staticContentTypeData.TypeData.Type.GetFileName(), new StandardEmitter().File(typeEmitter, staticContentTypeData.TypeData));
    }
}
