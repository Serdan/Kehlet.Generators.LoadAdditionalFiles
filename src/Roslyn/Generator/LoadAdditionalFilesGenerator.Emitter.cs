using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Kehlet.Generators.LoadAdditionalFiles.Generator.Data;

namespace Kehlet.Generators.LoadAdditionalFiles.Generator;

public partial class LoadAdditionalFilesGenerator
{
    internal class FilesEmitter(StaticContentTypeData typeData, ImmutableArray<FileData> texts) : SyntaxDescriptionEmitter
    {
        public Emitter Field(string memberName) =>
            Emitter * "public static readonly string " * memberName * " =";
        
        public Emitter Constant(string memberName) =>
            Emitter * "public const string " * memberName * " =";

        public Emitter Property(string memberName) =>
            Emitter * "public static string " * memberName * " =>";

        public Emitter Member(StaticContentOptions staticContentOptions, string path, string content)
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

            return string.IsNullOrEmpty(content) switch
            {
                true  => Emitter.Line(""" "";"""),
                false => Emitter.NewLine().RawStringLiteral(content)
            };
        }

        public void Members(StaticContentOptions staticContentOptions)
        {
            foreach (var (path, content) in texts)
            {
                if (staticContentOptions.RegexFilter is { Length: > 0 } pattern
                 && Regex.IsMatch(path, pattern, RegexOptions.Compiled) is false)
                {
                    continue;
                }

                Member(staticContentOptions, path, content).NewLine();
            }
        }

        public override Unit VisitNamedTypeBody(NamedTypeDescription description)
        {
            if (description.IsTargetNode is false)
            {
                return unit;
            }

            foreach (var targetOptions in typeData.Options)
            {
                Members(targetOptions);
            }

            return unit;
        }
    }
}
