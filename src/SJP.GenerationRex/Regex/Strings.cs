using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.GenerationRex.RegularExpressions;

internal static class Strings
{
    public static string AlternationCantCapture { get; } = "Alternation conditions do not capture and cannot be named.";

    public static string AlternationCantHaveComment { get; } = "Alternation conditions cannot be comments.";

    public static string BackRefCaptureGroupNotSupported { get; } = "Unsupported regular expression feature in use. Regular expressions cannot be generated when replacement references or capture groups are used.";

    public static string BadClassInCharRange { get; } = @"Cannot include class \\{0} in character range.";

    public static string CaptureGroupOutOfRange { get; } = "Capture group numbers must be less than or equal to Int32.MaxValue.";

    public static string IllegalCondition { get; } = "Illegal conditional (?(...)) expression.";

    public static string IllegalEndEscape { get; } = @"Illegal \\ at end of pattern.";

    public static string IllegalRange { get; } = "Illegal {x,y} with x > y.";

    public static string IncompleteSlashP { get; } = @"Incomplete \\p{X} character escape.";

    public static string InternalError { get; } = "Internal error in ScanRegex.";

    public static string InvalidGroupName { get; } = "Invalid group name: Group names must begin with a word character.";

    public static string MakeException { get; } = "parsing '{0}' - {1}";

    public static string MalformedNameRef { get; } = @"Malformed \\k<...> named back reference.";

    public static string MalformedSlashP { get; } = @"Malformed \\p{X} character escape.";

    public static string MissingControl { get; } = "Missing control character.";

    public static string NestedQuantify { get; } = "Nested quantifier {0}.";

    public static string NotEnoughParens { get; } = "Not enough )'s.";

    public static string QuantifyAfterNothing { get; } = "Quantifier {x,y} following nothing.";

    public static string ReversedCharRange { get; } = "[x-y] range in reverse order.";

    public static string SubtractionMustBeLast { get; } = "A subtraction must be the last element in a character class.";

    public static string TooFewHex { get; } = "Insufficient hexadecimal digits.";

    public static string TooManyAlternates { get; } = "Too many | in (?()|).";

    public static string TooManyParens { get; } = "Too many )'s.";

    public static string UnknownProperty { get; } = "Unknown property '{0}'.";

    public static string UnrecognizedControl { get; } = "Unrecognized control character.";

    public static string UnrecognizedEscape { get; } = @"Unrecognized escape sequence \\{0}.";

    public static string UnrecognizedGrouping { get; } = "Unrecognized grouping construct.";

    public static string UnterminatedBracket { get; } = "Unterminated [] set.";

    public static string UnterminatedComment { get; } = "Unterminated (?#...) comment.";
}
