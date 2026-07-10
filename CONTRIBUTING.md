# Contributing & Code Formatting

Most of these rules are enforced by [`.editorconfig`](.editorconfig). Please run a format pass before opening a PR.

### Keep the scope clear

Always use braces, even for single-line bodies.

This is ok:
```cs
if (!something)
{
    return;
}
```
This is not:
```cs
if (!something)
    return;
```

### Run the formatter

We use [CSharpier](https://csharpier.com/) to keep the project styled and formatted. Install it with `dotnet tool install -g csharpier` and run `csharpier format .` before opening a PR. A workflow will fail if formatting changes are required. See the [Style Guide](README.md#style-guide) in the README for format-on-save setup.

### Use file-scoped namespaces

This is ok:
```cs
namespace SPTarkov.Server.Core.Utils;

public class JsonUtil
{
}
```
This is not:
```cs
namespace SPTarkov.Server.Core.Utils
{
    public class JsonUtil
    {
    }
}
```

### Put `using` directives outside the namespace

`System.*` directives come first, then the rest sorted alphabetically. They belong above the namespace declaration, never inside it.

### Do not qualify members with `this.`

This is ok:
```cs
_logger.Debug(message);
```
This is not:
```cs
this._logger.Debug(this.message);
```

### Naming

- `private`/`internal` fields are `_camelCase` with a leading underscore.
- `const` fields are `PascalCase`.
- Prefer the language keyword over the BCL type (`string` not `String`, `int` not `Int32`).

### Avoid expression-bodied members

Use block bodies for methods, constructors, properties, and accessors. Expression-bodied lambdas are fine.

This is ok:
```cs
public int GetCount()
{
    return _items.Count;
}
```
This is not:
```cs
public int GetCount() => _items.Count;
```

### AI-Generated Code Policy

We do not allow AI-generated code from first-time contributors. We reserve the right to reject any submissions we suspect to be AI-generated.
The only exception is using AI to generate comments or documentation for code you write yourself.

### Ownership of Contributions

By submitting code to this repository, you agree that your contributions are licensed to the project under the terms of the [LICENSE](LICENSE) file. This gives the project the right to use, modify, and redistribute your contributions.
Once submitted, contributions cannot be removed or retracted, and the project may continue to use your code even if you later wish to withdraw it.
