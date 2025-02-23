using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Kehlet.Generators.Attributes;
using Kehlet.Generators.LoadAdditionalFiles.Data;
using Kehlet.SourceGenerator.Source;

namespace Kehlet.Generators.LoadAdditionalFiles;

internal static class AdditionalFilesGeneratorEmitter
{
    public static string GetRawStringQuotes(string file)
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

    public static IEmitter StringLiteral(this IEmitter emitter, string quotes, string content)
    {
        return emitter.WithIndent(0, out var indent)
                      .Line(quotes)
                      .Line(content)
                      .Line($"{quotes};")
                      .WithIndent(indent);
    }

    public static IEmitter Field(this IEmitter emitter, string memberName, string quotes, string sourceText)
    {
        emitter.Emit($"""public static readonly string {memberName} =""");
        return string.IsNullOrEmpty(sourceText)
            ? emitter.Line(""" "";""")
            : emitter.StringLiteral(quotes, sourceText);
    }

    public static IEmitter Constant(this IEmitter emitter, string memberName, string quotes, string sourceText)
    {
        emitter.Emit($"public const string {memberName} =");
        return string.IsNullOrEmpty(sourceText)
            ? emitter.Line(""" "";""")
            : emitter.StringLiteral(quotes, sourceText);
    }

    public static IEmitter Property(this IEmitter emitter, string memberName, string quotes, string sourceText)
    {
        emitter.Emit($"public static string {memberName} =>");
        return string.IsNullOrEmpty(sourceText)
            ? emitter.Line(""" "";""")
            : emitter.StringLiteral(quotes, sourceText);
    }

    public static IEmitter Member(this IEmitter emitter, TargetOptions targetOptions, string path, string content)
    {
        if (targetOptions.RegexFilter is { Length: > 0 } pattern && Regex.IsMatch(path, pattern, RegexOptions.Compiled) is false)
        {
            return emitter;
        }

        var fileName = targetOptions.OmitFileExtension
            ? Path.GetFileNameWithoutExtension(path)
            : Path.GetFileName(path);

        var memberName = fileName.Replace('.', '_').Replace('`', '_');
        memberName = $"{targetOptions.MemberNamePrefix}{memberName}{targetOptions.MemberNameSuffix}";

        var quotes = GetRawStringQuotes(content);

        return targetOptions.MemberKind switch
        {
            MemberKind.Field => emitter.Field(memberName, quotes, content),
            MemberKind.Constant => emitter.Constant(memberName, quotes, content),
            MemberKind.Property => emitter.Property(memberName, quotes, content),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static IEmitter Members(this IEmitter emitter, TargetOptions targetOptions, ImmutableArray<FileData> texts)
    {
        foreach (var (path, content) in texts)
        {
            emitter.Member(targetOptions, path, content);
        }

        return emitter;
    }

    public static IEmitter TypeBody(this IEmitter emitter, TargetData targetData, ImmutableArray<FileData> texts)
    {
        foreach (var targetOptions in targetData.FileTargets)
        {
            emitter.Members(targetOptions, texts);
        }

        return emitter;
    }

    public static IEmitter Type(this IEmitter emitter, TargetData targetData, ImmutableArray<FileData> texts)
    {
        return emitter.Line(targetData.TypeData.TypeDeclaration)
                      .OpenBrace()
                      .TypeBody(targetData, texts)
                      .CloseBrace();
    }

    public static IEmitter File(this IEmitter emitter, TargetData targetData, ImmutableArray<FileData> texts)
    {
        return emitter.NullableDirective()
                      .Line(targetData.TypeData.NamespaceDeclaration)
                      .Line()
                      .Type(targetData, texts);
    }
}
