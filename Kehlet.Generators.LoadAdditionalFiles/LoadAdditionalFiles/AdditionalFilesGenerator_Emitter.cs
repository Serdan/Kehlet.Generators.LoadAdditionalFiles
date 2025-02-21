using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Kehlet.Generators.Attributes;
using Kehlet.Generators.LoadAdditionalFiles.Data;

namespace Kehlet.Generators.LoadAdditionalFiles;

public partial class AdditionalFilesGenerator
{
    internal static class Emitter
    {
        public static Unit EmitField(string memberName, string quotes, string sourceText, StringBuilder builder)
        {
            const string memberDeclFormat = "    public static readonly string {0} =";
            const string emptyMemberFormat = $"""{memberDeclFormat} "";""";
            const string memberFormat =
                $$"""
                {{memberDeclFormat}}
                {1}
                {2}
                {1};

                """;

            if (string.IsNullOrEmpty(sourceText))
            {
                builder.AppendFormat(emptyMemberFormat, memberName);
                builder.AppendLine();
            }
            else
            {
                builder.AppendFormat(memberFormat, memberName, quotes, sourceText);
            }

            return unit;
        }

        public static Unit EmitConstant(string memberName, string quotes, string sourceText, StringBuilder builder)
        {
            const string memberDeclFormat = "    public const string {0} =";
            const string emptyMemberFormat = $"""{memberDeclFormat} "";""";
            const string memberFormat =
                $$"""
                {{memberDeclFormat}}
                {1}
                {2}
                {1};

                """;

            if (string.IsNullOrEmpty(sourceText))
            {
                builder.AppendFormat(emptyMemberFormat, memberName);
                builder.AppendLine();
            }
            else
            {
                builder.AppendFormat(memberFormat, memberName, quotes, sourceText);
            }

            return unit;
        }

        public static Unit EmitProperty(string memberName, string quotes, string sourceText, StringBuilder builder)
        {
            const string memberDeclFormat = "    public static string {0} =>";
            const string emptyMemberFormat = $"""{memberDeclFormat} "";""";
            const string memberFormat =
                $$"""
                {{memberDeclFormat}}
                {1}
                {2}
                {1};

                """;

            if (string.IsNullOrEmpty(sourceText))
            {
                builder.AppendFormat(emptyMemberFormat, memberName);
                builder.AppendLine();
            }
            else
            {
                builder.AppendFormat(memberFormat, memberName, quotes, sourceText);
            }

            return unit;
        }

        public static void EmitMembers(FileTarget fileTarget, ImmutableArray<FileData> texts, StringBuilder builder)
        {
            foreach ((string path, string content) in texts)
            {
                if (fileTarget.RegexFilter is { Length: > 0 } pattern && Regex.IsMatch(path, pattern, RegexOptions.Compiled) is false)
                {
                    continue;
                }

                var fileName = Path.GetFileName(path);
                if (fileTarget.OmitFileExtension)
                {
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                }

                var memberName = fileName.Replace('.', '_').Replace('`', '_');
                memberName = $"{fileTarget.MemberNamePrefix}{memberName}{fileTarget.MemberNameSuffix}";

                var quotes = GetRawStringQuotes(content);

                _ = fileTarget.MemberKind switch
                {
                    MemberKind.Field => EmitField(memberName, quotes, content, builder),
                    MemberKind.Constant => EmitConstant(memberName, quotes, content, builder),
                    MemberKind.Property => EmitProperty(memberName, quotes, content, builder),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

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
    }
}
