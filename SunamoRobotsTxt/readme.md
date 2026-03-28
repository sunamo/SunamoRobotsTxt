# SunamoRobotsTxt

A .NET library for parsing and generating robots.txt files.

## Features

- Parse existing robots.txt content from lines
- Build robots.txt files programmatically with Allow/Disallow rules and Sitemap entries
- Save generated robots.txt to a file

## Installation

```bash
dotnet add package SunamoRobotsTxt
```

## Usage

### Parsing an existing robots.txt

```csharp
var lines = File.ReadAllLines("robots.txt");
var builder = new RobotsTxtBuilder(lines);

// Access parsed data
var sitemaps = builder.Sitemaps;
var allows = builder.Allows;
var disallows = builder.Disallows;
```

### Building a new robots.txt

```csharp
var builder = new RobotsTxtBuilder(Array.Empty<string>());
builder.Sitemap("https://example.com/sitemap.xml");
builder.Allow("*", "/public/");
builder.Disallow("*", "/private/");
builder.Save("robots.txt");
```

## Target Frameworks

`net10.0`, `net9.0`, `net8.0`

## Links

- [NuGet](https://www.nuget.org/profiles/sunamo)
- [GitHub](https://github.com/sunamo/PlatformIndependentNuGetPackages)
- [Developer site](https://sunamo.cz)

## License

MIT
