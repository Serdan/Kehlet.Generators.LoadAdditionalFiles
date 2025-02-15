using Microsoft.CodeAnalysis;

namespace Kehlet.Generators.LoadAdditionalFiles;

[Embedded]
internal enum MemberKind
{
    Field,
    Constant,
    Property
}
