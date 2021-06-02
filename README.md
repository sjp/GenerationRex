<h1 align="center">
	<br>
	<img width="256" height="256" src="rex.png" alt="GenerationRex">
	<br>
	<br>
</h1>

> Efficiently generate data from a regular expression.

[![License (MIT)](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT) ![Build Status](https://github.com/sjp/GenerationRex/workflows/CI/badge.svg?branch=master) [![Code coverage](https://img.shields.io/codecov/c/gh/sjp/GenerationRex/master?logo=codecov)](https://codecov.io/gh/sjp/GenerationRex)

**GenerationRex** will generate members from a regular expression. This is particularly (but not only) useful for generating synthetic data for testing purposes. **GenerationRex** is an unofficial revival of the [Rex](https://www.microsoft.com/en-us/research/publication/rex-symbolic-regular-expression-explorer/) command line tool, originally published by Microsoft researchers. It has been repackaged into a class library, and made available for more runtimes (e.g. .NET Core and Standard).

When generating data from **GenerationRex**, all output will match the input regular expression. In other words the following identity is true:

```csharp
engine.GenerateMembers(@"^\d{5}$", 10)
	.All(member => Regex.IsMatch(member, @"^\d{5}$")); // true
```

## Highlights

* Supports .NET Standard 2.1.
* Handles generating regular expressions for multiple character sets.
* Most common regular expressions supported.

## Installation

**TODO** This is not yet correct, but will be once the package has been published on Nuget.

```powershell
Install-Package SJP.GenerationRex
```

or

```console
dotnet add package SJP.GenerationRex
```

## Usage

The key component is the `RexEngine`, which determines how members are generated.

```csharp
var rex = new RexEngine();
```

By default, it will generate members for the `ASCII` character set. To do this, use the `GenerateMembers()` method, which can take an integer limiting the amount of members to generate.

```csharp
var emailRegex = @"^\w+@\w+\.[a-z]]+$"; // incorrect, but sufficient for demo
IEnumerable<string> members = rex.GenerateMembers(emailRegex, 5);

// members contains the following values:
// 1. K@G.w
// 2. 1@Y81J3.rggl
// 3. 7W@7N.g
// 4. H3@7.p
// 5. dK@89L.eaz
```

The sequence can also be (potentially) infinite, as it will generate values until no more unique values can be generated for the regular expression. To do this, do not provide a limiting integer:

```csharp
// unlimited (but not necessarily infinite) results
IEnumerable<string> members = rex.GenerateMembers(emailRegex);
```

We can also provide different character sets (via `Encoding` objects), which can generate potentially surprising results. For example, consider a five digit integer.

```csharp
var rex = new RexEngine(Encoding.Unicode);
var numberRegex = @"^\d{5}$";
var numbers = rex.GenerateMembers(numberRegex, 5);

// numbers contains the following values:
// 1. １３᭘౮꩐
// 2. 1႘꯷꤆෯
// 3. ꘩߂８꘨8
// 4. ٢໔４９၉
// 5. ౧２꯸᪉４
```

The generated numbers are not only the Arabic numerals typically encountered, but also those from other numeral systems, e.g. Sinhala or Tamil.

## Remarks

Given how powerful regular expressions can be, care must be taken to ensure that results are sufficiently useful for a given use case. This usually means ensuring the regular expression is wrapped in `^` and `$` characters.

To demonstrate how to create better results, we can improve the email address generating regular expression. To do this, we will ensure only `a-z` characters are allowed for the local or user name component, rather than any word character (which can include numbers and punctuation). We will apply the same restriction to domain names. Finally, we'll make sure that the top-level domain name for the address is only two or three letter characters.

The results are the following:

 ```csharp
var rex = new RexEngine();
var emailRegex = @"^[a-z]+@[a-z]+\.([a-z]{2}|[a-z]{3})$"; // slightly improved email addresses
IEnumerable<string> members = rex.GenerateMembers(emailRegex, 5);

// members contains the following values:
// 1. xktunt@zm.sqq
// 2. nf@zkl.qf
// 3. szauo@xe.zz
// 4. japw@r.owz
// 5. ek@wt.art
```

## Icon

Icon created by [Freepik](http://www.freepik.com).
