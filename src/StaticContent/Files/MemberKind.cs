using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles.StaticContent;

[Embedded]
internal enum MemberKind
{
    Field,
    Constant,
    Property
}
