using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

public partial class AdditionalFilesGenerator
{
    internal class Emitter(StaticContentTypeData typeData, ImmutableArray<FileData> texts) : SyntaxDescriptionEmitter
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

        public Unit Field(string memberName) => Emitter.Tabs().Append($"public static readonly string {memberName} =").Ignore();

        public Unit Constant(string memberName) => Emitter.Tabs().Append($"public const string {memberName} =").Ignore();

        public Unit Property(string memberName) => Emitter.Tabs().Append($"public static string {memberName} =>").Ignore();

        public void Member(StaticContentOptions staticContentOptions, string path, string content)
        {
            var fileName = staticContentOptions.OmitFileExtension switch
            {
                true  => Path.GetFileNameWithoutExtension(path),
                false => Path.GetFileName(path)
            };

            var memberName = staticContentOptions.MemberNamePrefix
                           + fileName.Replace('.', '_').Replace('`', '_')
                           + staticContentOptions.MemberNameSuffix;

            _ = staticContentOptions.MemberKind switch
            {
                MemberKind.Field    => Field(memberName),
                MemberKind.Constant => Constant(memberName),
                MemberKind.Property => Property(memberName),
                _                   => throw new ArgumentOutOfRangeException()
            };

            _ = string.IsNullOrEmpty(content) switch
            {
                true  => Emitter.Line(""" "";"""),
                false => Emitter.NewLine().StringLiteral(GetRawStringQuotes(content), content)
            };
        }

        public void Members(StaticContentOptions staticContentOptions)
        {
            foreach (var (path, content) in texts)
            {
                if (staticContentOptions.RegexFilter is { Length: > 0 } pattern && Regex.IsMatch(path, pattern, RegexOptions.Compiled) is false)
                {
                    continue;
                }

                Member(staticContentOptions, path, content);
                Emitter.NewLine();
            }
        }

        public override Option<IEmitter> VisitNamedTypeBody(NamedTypeDescription description)
        {
            if (description.Identifier != typeData.TargetIdentifier)
            {
                return Emitter.Some();
            }

            foreach (var targetOptions in typeData.Options)
            {
                Members(targetOptions);
            }

            return Emitter.Some();
        }
    }
}
