Annotate partial class with `LoadAdditionalFilesAttribute` to have additional files in the project statically loaded into const string fields of the class.

Notably this is a safe and performant way for source generators to load static files.

Source:
```csharp
[LoadAdditionalFiles(RegexFilter = "\.md$", PropertyNameSuffix = "Markdown")]
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
