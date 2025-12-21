### SunamoFtp

Part of PlatformIndependentNuGetPackages:

- [nuget.org](https://www.nuget.org/profiles/sunamo)
- [github.org](https://github.com/sunamo/PlatformIndependentNuGetPackages)

Another links:

- [Developer site](https://sunamo.cz)

Request for new features / bug report / etc: [Mail](mailto:radek.jancik@sunamo.cz) or on GitHub
## Target Frameworks

**TargetFrameworks:** `net10.0;net9.0;net8.0`

**Reason:** Code uses C# 12.0 features (collection expressions, primary constructors) or dependencies requiring .NET 8.0+:
- Collection expressions `[]` syntax requires C# 12.0 (net8.0+)
- Primary constructors require C# 12.0 (net8.0+) 
- Entity Framework Core 9.x requires net8.0+
