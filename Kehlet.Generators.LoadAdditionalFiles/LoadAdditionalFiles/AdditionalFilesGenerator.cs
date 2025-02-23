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
        context.RegisterStaticType<LoadAdditionalFilesAttribute>(LoadAdditionalFilesAttributeSource);
        context.RegisterStaticType<MemberKind>(MemberKindSource);

        var (textValues, textErrors) = context.AdditionalTextsProvider.Select(Parser.GetText).Partition();
        var (typeValues, typeErrors) = context.SyntaxForAttribute(AttributeFullName, SyntaxTarget.Type, Parser.Parse).Partition();

        var typeValuesWithTexts = typeValues.Combine(textValues.Collect().WithComparer(Equality<FileData>.ArrayComparer));

        context.RegisterSourceOutput(typeValuesWithTexts, static (ctx, tuple) => GenerateCode(ctx, tuple.Left, tuple.Right));
        context.RegisterSourceOutput(typeErrors, ReportDiagnostics);
        context.RegisterSourceOutput(textErrors, ReportDiagnostics);
    }

    internal static void ReportDiagnostics(SourceProductionContext context, IDiagnostic error) =>
        error.Report(context);

    internal static void GenerateCode(SourceProductionContext context, TargetData targetData, ImmutableArray<FileData> texts) =>
        context.AddSourceUTF8(targetData.TypeData.GetFileName(), Emitter.File(targetData, texts));
}
