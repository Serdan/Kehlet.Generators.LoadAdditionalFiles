using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

internal static class EmitterExtensions
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
        emitter.Tabs().Append($"public static readonly string {memberName} =");
        return string.IsNullOrEmpty(sourceText)
            ? emitter.Line(""" "";""")
            : emitter.NewLine().StringLiteral(quotes, sourceText);
    }

    public static IEmitter Constant(this IEmitter emitter, string memberName, string quotes, string sourceText)
    {
        emitter.Tabs().Append($"public const string {memberName} =");
        return string.IsNullOrEmpty(sourceText)
            ? emitter.Line(""" "";""")
            : emitter.NewLine().StringLiteral(quotes, sourceText);
    }

    public static IEmitter Property(this IEmitter emitter, string memberName, string quotes, string sourceText)
    {
        emitter.Tabs().Append($"public static string {memberName} =>");
        return string.IsNullOrEmpty(sourceText)
            ? emitter.Line(""" "";""")
            : emitter.NewLine().StringLiteral(quotes, sourceText);
    }

    public static IEmitter Member(this IEmitter emitter, StaticContentOptions staticContentOptions, string path, string content)
    {
        var fileName = staticContentOptions.OmitFileExtension
            ? Path.GetFileNameWithoutExtension(path)
            : Path.GetFileName(path);

        var memberName = fileName.Replace('.', '_').Replace('`', '_');
        memberName = $"{staticContentOptions.MemberNamePrefix}{memberName}{staticContentOptions.MemberNameSuffix}";

        var quotes = GetRawStringQuotes(content);

        return staticContentOptions.MemberKind switch
        {
            MemberKind.Field => emitter.Field(memberName, quotes, content),
            MemberKind.Constant => emitter.Constant(memberName, quotes, content),
            MemberKind.Property => emitter.Property(memberName, quotes, content),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static IEmitter Members(this IEmitter emitter, StaticContentOptions staticContentOptions, ImmutableArray<FileData> texts)
    {
        foreach (var (path, content) in texts)
        {
            if (staticContentOptions.RegexFilter is { Length: > 0 } pattern && Regex.IsMatch(path, pattern, RegexOptions.Compiled) is false)
            {
                continue;
            }

            emitter.Member(staticContentOptions, path, content)
                   .NewLine();
        }

        return emitter;
    }
}

public partial class AdditionalFilesGenerator
{
    internal class Emitter(StaticContentTypeData staticContentTypeData, ImmutableArray<FileData> texts) : TypeEmitter
    {
        public override IEmitter TypeBody(IEmitter emitter)
        {
            foreach (var targetOptions in staticContentTypeData.Options)
            {
                emitter.Members(targetOptions, texts);
            }

            return emitter;
        }
    }
}
