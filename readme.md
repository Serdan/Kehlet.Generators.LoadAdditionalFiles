Annotate a partial class with `LoadAdditionalFilesAttribute` to have additional files in the project statically loaded into members of the class.

Notably this is a safe and performant way for source generators to load static files.

Source:
```csharp
[LoadAdditionalFiles(RegexFilter = "\.md$", MemberNameSuffix = "Markdown")]
public static partial class MyFiles;
```

Additional file:
```markdown
# Hi! This is a readme!
```

Project:
```
    <AdditionalFiles Include="Readme.md"/>
```

Generated:
```csharp
static partial class MyFiles
{
    public const string ReadmeMarkdown =
"""
# Hi! This is a readme!
""";
}
```
