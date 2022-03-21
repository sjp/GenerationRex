// This RegexParser class is internal to the Regex package.
// It builds a tree of RegexNodes from a regular expression

// Implementation notes:
//
// It would be nice to get rid of the comment modes, since the
// ScanBlank() calls are just kind of duct-taped in.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using EnumsNET;

namespace SJP.GenerationRex.RegularExpressions;

internal sealed class RegexParser
{
    internal RegexNode _stack;
    internal RegexNode _group;
    internal RegexNode _alternation;
    internal RegexNode _concatenation;
    internal RegexNode _unit;

    internal string _pattern;
    internal int _currentPos;
    internal CultureInfo _culture;

    internal int _autocap;

    internal RegexOptions _options;
    internal List<RegexOptions> _optionsStack;

    internal bool _ignoreNextParen;

    internal const int MaxValueDiv10 = int.MaxValue / 10;
    internal const int MaxValueMod10 = int.MaxValue % 10;

    /*
     * This static call constructs a RegexTree from a regular expression
     * pattern string and an option string.
     *
     * The method creates, drives, and drops a parser instance.
     */
    internal static RegexTree Parse(string re, RegexOptions op)
    {
        var culture = op.HasAnyFlags(RegexOptions.CultureInvariant)
            ? CultureInfo.InvariantCulture
            : CultureInfo.CurrentCulture;
        var p = new RegexParser(culture)
        {
            _options = op
        };

        p.SetPattern(re);
        p.Reset(op);
        var root = p.ScanRegex();

        return new RegexTree(root, op);
    }

    /*
     * Private constructor.
     */
    private RegexParser(CultureInfo culture)
    {
        _culture = culture;
        _optionsStack = new List<RegexOptions>();
    }

    /*
     * Drops a string into the pattern buffer.
     */
    private void SetPattern(string Re)
    {
        if (Re == null)
            Re = string.Empty;
        _pattern = Re;
        _currentPos = 0;
    }

    /*
     * Resets parsing to the beginning of the pattern.
     */
    private void Reset(RegexOptions topopts)
    {
        _currentPos = 0;
        _autocap = 1;
        _ignoreNextParen = false;

        if (_optionsStack.Count > 0)
            _optionsStack.RemoveRange(0, _optionsStack.Count - 1);

        _options = topopts;
        _stack = null;
    }

    /*
     * The main parsing function.
     */
    private RegexNode ScanRegex()
    {
        var ch = '@'; // nonspecial ch, means at beginning
        var isQuantifier = false;

        StartGroup(new RegexNode(RegexNode.Capture, _options, 0, -1));

        while (CharsRight() > 0)
        {
            var wasPrevQuantifier = isQuantifier;
            isQuantifier = false;

            ScanBlank();

            var startpos = Textpos();

            // move past all of the normal characters.  We'll stop when we hit some kind of control character,
            // or if IgnorePatternWhiteSpace is on, we'll stop when we see some whitespace.
            if (UseOptionX())
            {
                while (CharsRight() > 0 && (!IsStopperX(ch = RightChar()) || (ch == '{' && !IsTrueQuantifier())))
                    MoveRight();
            }
            else
            {
                while (CharsRight() > 0 && (!IsSpecial(ch = RightChar()) || (ch == '{' && !IsTrueQuantifier())))
                    MoveRight();
            }

            var endpos = Textpos();

            ScanBlank();

            if (CharsRight() == 0)
            {
                ch = '!'; // nonspecial, means at end
            }
            else if (IsSpecial(ch = RightChar()))
            {
                isQuantifier = IsQuantifier(ch);
                MoveRight();
            }
            else
            {
                ch = ' '; // nonspecial, means at ordinary char
            }

            if (startpos < endpos)
            {
                var cchUnquantified = endpos - startpos - (isQuantifier ? 1 : 0);

                wasPrevQuantifier = false;

                if (cchUnquantified > 0)
                    AddConcatenate(startpos, cchUnquantified);

                if (isQuantifier)
                    AddUnitOne(CharAt(endpos - 1));
            }

            switch (ch)
            {
                case '!':
                    goto BreakOuterScan;

                case ' ':
                    goto ContinueOuterScan;

                case '[':
                    AddUnitSet(ScanCharClass(UseOptionI()).ToStringClass());
                    break;

                case '(':
                    {
                        RegexNode grouper;

                        PushOptions();

                        if ((grouper = ScanGroupOpen()) == null)
                        {
                            PopKeepOptions();
                        }
                        else
                        {
                            PushGroup();
                            StartGroup(grouper);
                        }
                    }
                    continue;

                case '|':
                    AddAlternate();
                    goto ContinueOuterScan;

                case ')':
                    if (EmptyStack())
                        throw MakeException(Strings.TooManyParens);

                    AddGroup();
                    PopGroup();
                    PopOptions();

                    if (Unit() == null)
                        goto ContinueOuterScan;
                    break;

                case '\\':
                    AddUnitNode(ScanBackslash());
                    break;

                case '^':
                    AddUnitType(UseOptionM() ? RegexNode.Bol : RegexNode.Beginning);
                    break;

                case '$':
                    AddUnitType(UseOptionM() ? RegexNode.Eol : RegexNode.EndZ);
                    break;

                case '.':
                    if (UseOptionS())
                        AddUnitSet(RegexCharClass.AnyClass);
                    else
                        AddUnitNotone('\n');
                    break;

                case '{':
                case '*':
                case '+':
                case '?':
                    if (Unit() == null)
                    {
                        throw MakeException(wasPrevQuantifier ?
                                           string.Format(Strings.NestedQuantify, ch.ToString()) :
                                           Strings.QuantifyAfterNothing);
                    }

                    MoveLeft();
                    break;

                default:
                    throw MakeException(Strings.InternalError);
            }

            ScanBlank();

            if (CharsRight() == 0 || !(isQuantifier = IsTrueQuantifier()))
            {
                AddConcatenate();
                goto ContinueOuterScan;
            }

            ch = MoveRightGetChar();

            // Handle quantifiers
            while (Unit() != null)
            {
                int min;
                int max;
                bool lazy;

                switch (ch)
                {
                    case '*':
                        min = 0;
                        max = int.MaxValue;
                        break;

                    case '?':
                        min = 0;
                        max = 1;
                        break;

                    case '+':
                        min = 1;
                        max = int.MaxValue;
                        break;

                    case '{':
                        {
                            startpos = Textpos();
                            max = min = ScanDecimal();
                            if (startpos < Textpos())
                            {
                                if (CharsRight() > 0 && RightChar() == ',')
                                {
                                    MoveRight();
                                    if (CharsRight() == 0 || RightChar() == '}')
                                        max = int.MaxValue;
                                    else
                                        max = ScanDecimal();
                                }
                            }

                            if (startpos == Textpos() || CharsRight() == 0 || MoveRightGetChar() != '}')
                            {
                                AddConcatenate();
                                Textto(startpos - 1);
                                goto ContinueOuterScan;
                            }
                        }

                        break;

                    default:
                        throw MakeException(Strings.InternalError);
                }

                ScanBlank();

                if (CharsRight() == 0 || RightChar() != '?')
                {
                    lazy = false;
                }
                else
                {
                    MoveRight();
                    lazy = true;
                }

                if (min > max)
                    throw MakeException(Strings.IllegalRange);

                AddConcatenate(lazy, min, max);
            }

        ContinueOuterScan:
            ;
        }

    BreakOuterScan:
        ;

        if (!EmptyStack())
            throw MakeException(Strings.NotEnoughParens);

        AddGroup();

        return Unit();
    }

    /*
     * Scans contents of [] (not including []'s), and converts to a
     * RegexCharClass.
     */
    private RegexCharClass ScanCharClass(bool caseInsensitive)
    {
        return ScanCharClass(caseInsensitive, false);
    }

    /*
     * Scans contents of [] (not including []'s), and converts to a
     * RegexCharClass.
     */
    private RegexCharClass ScanCharClass(bool caseInsensitive, bool scanOnly)
    {
        var ch = '\0';
        var chPrev = '\0';
        var inRange = false;
        var firstChar = true;
        var closed = false;

        var cc = scanOnly ? null : new RegexCharClass();

        if (CharsRight() > 0 && RightChar() == '^')
        {
            MoveRight();
            if (!scanOnly)
                cc.Negate = true;
        }

        for (; CharsRight() > 0; firstChar = false)
        {
            var fTranslatedChar = false;
            ch = MoveRightGetChar();
            if (ch == ']')
            {
                if (!firstChar)
                {
                    closed = true;
                    break;
                }
            }
            else if (ch == '\\' && CharsRight() > 0)
            {
                switch (ch = MoveRightGetChar())
                {
                    case 'D':
                    case 'd':
                        if (!scanOnly)
                        {
                            if (inRange)
                                throw MakeException(string.Format(Strings.BadClassInCharRange, ch.ToString()));
                            cc.AddDigit(UseOptionE(), ch == 'D', _pattern);
                        }
                        continue;

                    case 'S':
                    case 's':
                        if (!scanOnly)
                        {
                            if (inRange)
                                throw MakeException(string.Format(Strings.BadClassInCharRange, ch.ToString()));
                            cc.AddSpace(UseOptionE(), ch == 'S');
                        }
                        continue;

                    case 'W':
                    case 'w':
                        if (!scanOnly)
                        {
                            if (inRange)
                                throw MakeException(string.Format(Strings.BadClassInCharRange, ch.ToString()));

                            cc.AddWord(UseOptionE(), ch == 'W');
                        }
                        continue;

                    case 'p':
                    case 'P':
                        if (!scanOnly)
                        {
                            if (inRange)
                                throw MakeException(string.Format(Strings.BadClassInCharRange, ch.ToString()));
                            cc.AddCategoryFromName(ParseProperty(), ch != 'p', caseInsensitive, _pattern);
                        }
                        else
                        {
                            ParseProperty();
                        }

                        continue;

                    case '-':
                        if (!scanOnly)
                            cc.AddRange(ch, ch);
                        continue;

                    default:
                        MoveLeft();
                        ch = ScanCharEscape(); // non-literal character
                        fTranslatedChar = true;
                        break;          // this break will only break out of the switch
                }
            }
            else if (ch == '[')
            {
                // This is code for Posix style properties - [:Ll:] or [:IsTibetan:].
                // It currently doesn't do anything other than skip the whole thing!
                if (CharsRight() > 0 && RightChar() == ':' && !inRange)
                {
                    var savePos = Textpos();

                    MoveRight();
                    _ = ScanCapname();
                    if (CharsRight() < 2 || MoveRightGetChar() != ':' || MoveRightGetChar() != ']')
                        Textto(savePos);
                    // else lookup name (nyi)
                }
            }

            if (inRange)
            {
                inRange = false;
                if (!scanOnly)
                {
                    if (ch == '[' && !fTranslatedChar && !firstChar)
                    {
                        // We thought we were in a range, but we're actually starting a subtraction.
                        // In that case, we'll add chPrev to our char class, skip the opening [, and
                        // scan the new character class recursively.
                        cc.AddChar(chPrev);
                        cc.AddSubtraction(ScanCharClass(caseInsensitive, false));

                        if (CharsRight() > 0 && RightChar() != ']')
                            throw MakeException(Strings.SubtractionMustBeLast);
                    }
                    else
                    {
                        // a regular range, like a-z
                        if (chPrev > ch)
                            throw MakeException(Strings.ReversedCharRange);
                        cc.AddRange(chPrev, ch);
                    }
                }
            }
            else if (CharsRight() >= 2 && RightChar() == '-' && RightChar(1) != ']')
            {
                // this could be the start of a range
                chPrev = ch;
                inRange = true;
                MoveRight();
            }
            else if (CharsRight() >= 1 && ch == '-' && !fTranslatedChar && RightChar() == '[' && !firstChar)
            {
                // we aren't in a range, and now there is a subtraction.  Usually this happens
                // only when a subtraction follows a range, like [a-z-[b]]
                if (!scanOnly)
                {
                    MoveRight(1);
                    cc.AddSubtraction(ScanCharClass(caseInsensitive, false));

                    if (CharsRight() > 0 && RightChar() != ']')
                        throw MakeException(Strings.SubtractionMustBeLast);
                }
                else
                {
                    MoveRight(1);
                    ScanCharClass(caseInsensitive, true);
                }
            }
            else
            {
                if (!scanOnly)
                    cc.AddRange(ch, ch);
            }
        }

        if (!closed)
            throw MakeException(Strings.UnterminatedBracket);

        if (!scanOnly && caseInsensitive)
            cc.AddLowercase(_culture);

        return cc;
    }

    /*
     * Scans chars following a '(' (not counting the '('), and returns
     * a RegexNode for the type of group scanned, or null if the group
     * simply changed options (?cimsx-cimsx) or was a comment (#...).
     */
    private RegexNode ScanGroupOpen()
    {
        var ch = '\0';
        int NodeType;
        var close = '>';

        // just return a RegexNode if we have:
        // 1. "(" followed by nothing
        // 2. "(x" where x != ?
        // 3. "(?)"
        if (CharsRight() == 0 || RightChar() != '?' || (RightChar() == '?' && (CharsRight() > 1 && RightChar(1) == ')')))
        {
            if (UseOptionN() || _ignoreNextParen)
            {
                _ignoreNextParen = false;
                return new RegexNode(RegexNode.Group, _options);
            }
            else
            {
                return new RegexNode(RegexNode.Capture, _options, _autocap++, -1);
            }
        }

        MoveRight();

        for (; ; )
        {
            if (CharsRight() == 0)
                break;

            switch (_ = MoveRightGetChar())
            {
                case ':':
                    NodeType = RegexNode.Group;
                    break;

                case '=':
                    _options &= ~(RegexOptions.RightToLeft);
                    NodeType = RegexNode.Require;
                    break;

                case '!':
                    _options &= ~(RegexOptions.RightToLeft);
                    NodeType = RegexNode.Prevent;
                    break;

                case '>':
                    NodeType = RegexNode.Greedy;
                    break;

                case '\'':
                    close = '\'';
                    goto case '<';
                // fallthrough

                case '<':
                    if (CharsRight() == 0)
                        goto BreakRecognize;

                    switch (ch = MoveRightGetChar())
                    {
                        case '=':
                            if (close == '\'')
                                goto BreakRecognize;

                            _options |= RegexOptions.RightToLeft;
                            NodeType = RegexNode.Require;
                            break;

                        case '!':
                            if (close == '\'')
                                goto BreakRecognize;

                            _options |= RegexOptions.RightToLeft;
                            NodeType = RegexNode.Prevent;
                            break;

                        default:
                            MoveLeft();
                            const int capnum = -1;
                            const int uncapnum = -1;
                            var proceed = false;

                            // grab part before -

                            if (ch >= '0' && ch <= '9')
                            {
                                throw MakeException(Strings.BackRefCaptureGroupNotSupported);
                            }
                            else if (RegexCharClass.IsWordChar(ch))
                            {
                                throw MakeException(Strings.BackRefCaptureGroupNotSupported);
                            }
                            else if (ch == '-')
                            {
                                proceed = true;
                            }
                            else
                            {
                                // bad group name - starts with something other than a word character and isn't a number
                                throw MakeException(Strings.InvalidGroupName);
                            }

                            // grab part after - if any

                            if ((capnum != -1 || proceed) && CharsRight() > 0 && RightChar() == '-')
                            {
                                MoveRight();
                                ch = RightChar();

                                if (ch >= '0' && ch <= '9')
                                {
                                    throw MakeException(Strings.BackRefCaptureGroupNotSupported);
                                }
                                else if (RegexCharClass.IsWordChar(ch))
                                {
                                    throw MakeException(Strings.BackRefCaptureGroupNotSupported);
                                }
                                else
                                {
                                    // bad group name - starts with something other than a word character and isn't a number
                                    throw MakeException(Strings.InvalidGroupName);
                                }
                            }

                            // actually make the node

                            if ((capnum != -1 || uncapnum != -1) && CharsRight() > 0 && MoveRightGetChar() == close)
                            {
                                return new RegexNode(RegexNode.Capture, _options, capnum, uncapnum);
                            }
                            goto BreakRecognize;
                    }
                    break;

                case '(':
                    // alternation construct (?(...) | )

                    var parenPos = Textpos();
                    if (CharsRight() > 0)
                    {
                        ch = RightChar();

                        // check if the alternation condition is a backref
                        if (ch >= '0' && ch <= '9')
                        {
                            throw MakeException(Strings.BackRefCaptureGroupNotSupported);
                        }
                        else if (RegexCharClass.IsWordChar(ch))
                        {
                            throw MakeException(Strings.BackRefCaptureGroupNotSupported);
                        }
                    }
                    // not a backref
                    NodeType = RegexNode.Testgroup;
                    Textto(parenPos - 1);       // jump to the start of the parentheses
                    _ignoreNextParen = true;    // but make sure we don't try to capture the insides

                    var charsRight = CharsRight();
                    if (charsRight >= 3 && RightChar(1) == '?')
                    {
                        var rightchar2 = RightChar(2);
                        // disallow comments in the condition
                        if (rightchar2 == '#')
                            throw MakeException(Strings.AlternationCantHaveComment);

                        // disallow named capture group (?<..>..) in the condition
                        if (rightchar2 == '\'')
                        {
                            throw MakeException(Strings.AlternationCantCapture);
                        }
                        else if (charsRight >= 4 && (rightchar2 == '<' && RightChar(3) != '!' && RightChar(3) != '='))
                        {
                            throw MakeException(Strings.AlternationCantCapture);
                        }
                    }

                    break;

                default:
                    MoveLeft();

                    NodeType = RegexNode.Group;
                    // Disallow options in the children of a testgroup node
                    if (_group._type != RegexNode.Testgroup)
                        ScanOptions();
                    if (CharsRight() == 0)
                        goto BreakRecognize;

                    if ((ch = MoveRightGetChar()) == ')')
                        return null;

                    if (ch != ':')
                        goto BreakRecognize;
                    break;
            }

            return new RegexNode(NodeType, _options);
        }

    BreakRecognize:
        // break Recognize comes here
        throw MakeException(Strings.UnrecognizedGrouping);
    }

    /*
     * Scans whitespace or x-mode comments.
     */
    private void ScanBlank()
    {
        if (UseOptionX())
        {
            for (; ; )
            {
                while (CharsRight() > 0 && IsSpace(RightChar()))
                    MoveRight();

                if (CharsRight() == 0)
                    break;

                if (RightChar() == '#')
                {
                    while (CharsRight() > 0 && RightChar() != '\n')
                        MoveRight();
                }
                else if (CharsRight() >= 3 && RightChar(2) == '#'
                         && RightChar(1) == '?' && RightChar() == '(')
                {
                    while (CharsRight() > 0 && RightChar() != ')')
                        MoveRight();
                    if (CharsRight() == 0)
                        throw MakeException(Strings.UnterminatedComment);
                    MoveRight();
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            for (; ; )
            {
                if (CharsRight() < 3 || RightChar(2) != '#'
                    || RightChar(1) != '?' || RightChar() != '(')
                {
                    return;
                }

                while (CharsRight() > 0 && RightChar() != ')')
                    MoveRight();
                if (CharsRight() == 0)
                    throw MakeException(Strings.UnterminatedComment);
                MoveRight();
            }
        }
    }

    /*
     * Scans chars following a '\' (not counting the '\'), and returns
     * a RegexNode for the type of atom scanned.
     */
    private RegexNode ScanBackslash()
    {
        char ch;
        RegexCharClass cc;

        if (CharsRight() == 0)
            throw MakeException(Strings.IllegalEndEscape);

        switch (ch = RightChar())
        {
            case 'b':
            case 'B':
            case 'A':
            case 'G':
            case 'Z':
            case 'z':
                MoveRight();
                return new RegexNode(TypeFromCode(ch), _options);

            case 'w':
                MoveRight();
                if (UseOptionE())
                    return new RegexNode(RegexNode.Set, _options, RegexCharClass.ECMAWordClass);
                return new RegexNode(RegexNode.Set, _options, RegexCharClass.WordClass);

            case 'W':
                MoveRight();
                if (UseOptionE())
                    return new RegexNode(RegexNode.Set, _options, RegexCharClass.NotECMAWordClass);
                return new RegexNode(RegexNode.Set, _options, RegexCharClass.NotWordClass);

            case 's':
                MoveRight();
                if (UseOptionE())
                    return new RegexNode(RegexNode.Set, _options, RegexCharClass.ECMASpaceClass);
                return new RegexNode(RegexNode.Set, _options, RegexCharClass.SpaceClass);

            case 'S':
                MoveRight();
                if (UseOptionE())
                    return new RegexNode(RegexNode.Set, _options, RegexCharClass.NotECMASpaceClass);
                return new RegexNode(RegexNode.Set, _options, RegexCharClass.NotSpaceClass);

            case 'd':
                MoveRight();
                if (UseOptionE())
                    return new RegexNode(RegexNode.Set, _options, RegexCharClass.ECMADigitClass);
                return new RegexNode(RegexNode.Set, _options, RegexCharClass.DigitClass);

            case 'D':
                MoveRight();
                if (UseOptionE())
                    return new RegexNode(RegexNode.Set, _options, RegexCharClass.NotECMADigitClass);
                return new RegexNode(RegexNode.Set, _options, RegexCharClass.NotDigitClass);

            case 'p':
            case 'P':
                MoveRight();
                cc = new RegexCharClass();
                cc.AddCategoryFromName(ParseProperty(), ch != 'p', UseOptionI(), _pattern);
                if (UseOptionI())
                    cc.AddLowercase(_culture);

                return new RegexNode(RegexNode.Set, _options, cc.ToStringClass());

            default:
                return ScanBasicBackslash();
        }
    }

    /*
     * Scans \-style backreferences and character escapes
     */
    private RegexNode ScanBasicBackslash()
    {
        if (CharsRight() == 0)
            throw MakeException(Strings.IllegalEndEscape);

        char ch;
        var angled = false;
        var close = '\0';
        var backpos = Textpos();
        ch = RightChar();

        // allow \k<foo> instead of \<foo>, which is now deprecated

        if (ch == 'k')
        {
            if (CharsRight() >= 2)
            {
                MoveRight();
                ch = MoveRightGetChar();

                if (ch == '<' || ch == '\'')
                {
                    angled = true;
                    close = (ch == '\'') ? '\'' : '>';
                }
            }

            if (!angled || CharsRight() <= 0)
                throw MakeException(Strings.MalformedNameRef);

            ch = RightChar();
        }

        // Note angle without \g

        else if ((ch == '<' || ch == '\'') && CharsRight() > 1)
        {
            angled = true;
            close = (ch == '\'') ? '\'' : '>';

            MoveRight();
            ch = RightChar();
        }

        // Try to parse backreference: \<1> or \<cap>

        if (angled && ch >= '0' && ch <= '9')
        {
            _ = ScanDecimal();

            if (CharsRight() > 0 && MoveRightGetChar() == close)
            {
                throw MakeException(Strings.BackRefCaptureGroupNotSupported);
            }
        }

        // Try to parse backreference or octal: \1

        else if (!angled && ch >= '1' && ch <= '9')
        {
            if (UseOptionE())
            {
                throw MakeException(Strings.BackRefCaptureGroupNotSupported);
            }
            else
            {
                throw MakeException(Strings.BackRefCaptureGroupNotSupported);
            }
        }
        else if (angled && RegexCharClass.IsWordChar(ch))
        {
            throw MakeException(Strings.BackRefCaptureGroupNotSupported);
        }

        // Not backreference: must be char code

        Textto(backpos);
        ch = ScanCharEscape();

        if (UseOptionI())
            ch = _culture.TextInfo.ToLower(ch);

        return new RegexNode(RegexNode.One, _options, ch);
    }

    /*
     * Scans a capture name: consumes word chars
     */
    private string ScanCapname()
    {
        var startpos = Textpos();

        while (CharsRight() > 0)
        {
            if (!RegexCharClass.IsWordChar(MoveRightGetChar()))
            {
                MoveLeft();
                break;
            }
        }

        return _pattern[startpos..Textpos()];
    }

    /*
     * Scans up to three octal digits (stops before exceeding 0377).
     */
    private char ScanOctal()
    {
        int d;
        int i;

        // Consume octal chars only up to 3 digits and value 0377

        var c = 3;
        if (c > CharsRight())
            c = CharsRight();

        for (i = 0; c > 0 && unchecked((uint)(d = RightChar() - '0')) <= 7; c--)
        {
            MoveRight();
            i *= 8;
            i += d;
            if (UseOptionE() && i >= 0x20)
                break;
        }

        // Octal codes only go up to 255.  Any larger and the behavior that Perl follows
        // is simply to truncate the high bits.
        i &= 0xFF;

        return (char)i;
    }

    /*
     * Scans any number of decimal digits (pegs value at 2^31-1 if too large)
     */
    private int ScanDecimal()
    {
        var i = 0;
        int d;

        while (CharsRight() > 0 && unchecked((uint)(d = (char)(RightChar() - '0'))) <= 9)
        {
            MoveRight();

            if (i > MaxValueDiv10 || (i == MaxValueDiv10 && d > MaxValueMod10))
                throw MakeException(Strings.CaptureGroupOutOfRange);

            i *= 10;
            i += d;
        }

        return i;
    }

    /*
     * Scans exactly c hex digits (c=2 for \xFF, c=4 for \uFFFF)
     */
    private char ScanHex(int c)
    {
        int i;
        int d;

        i = 0;

        if (CharsRight() >= c)
        {
            for (; c > 0 && ((d = HexDigit(MoveRightGetChar())) >= 0); c--)
            {
                i *= 0x10;
                i += d;
            }
        }

        if (c > 0)
            throw MakeException(Strings.TooFewHex);

        return (char)i;
    }

    /*
     * Returns n <= 0xF for a hex digit.
     */
    private static int HexDigit(char ch)
    {
        int d;

        if ((uint)(d = ch - '0') <= 9)
            return d;

        if (unchecked((uint)(d = ch - 'a')) <= 5)
            return d + 0xa;

        if ((uint)(d = ch - 'A') <= 5)
            return d + 0xa;

        return -1;
    }

    /*
     * Grabs and converts an ASCII control character
     */
    private char ScanControl()
    {
        char ch;

        if (CharsRight() <= 0)
            throw MakeException(Strings.MissingControl);

        ch = MoveRightGetChar();

        // \ca interpreted as \cA

        if (ch >= 'a' && ch <= 'z')
            ch = (char)(ch - ('a' - 'A'));

        if (unchecked(ch = (char)(ch - '@')) < ' ')
            return ch;

        throw MakeException(Strings.UnrecognizedControl);
    }

    /*
     * Returns true for options allowed only at the top level
     */
    private static bool IsOnlyTopOption(RegexOptions option)
    {
        return option == RegexOptions.RightToLeft
            || option == RegexOptions.CultureInvariant
            || option == RegexOptions.ECMAScript;
    }

    /*
     * Scans cimsx-cimsx option string, stops at the first unrecognized char.
     */
    private void ScanOptions()
    {
        char ch;
        bool off;
        RegexOptions option;

        for (off = false; CharsRight() > 0; MoveRight())
        {
            ch = RightChar();

            if (ch == '-')
            {
                off = true;
            }
            else if (ch == '+')
            {
                off = false;
            }
            else
            {
                option = OptionFromCode(ch);
                if (option == 0 || IsOnlyTopOption(option))
                    return;

                if (off)
                    _options &= ~option;
                else
                    _options |= option;
            }
        }
    }

    /*
     * Scans \ code for escape codes that map to single Unicode chars.
     */
    private char ScanCharEscape()
    {
        var ch = MoveRightGetChar();

        if (ch >= '0' && ch <= '7')
        {
            MoveLeft();
            return ScanOctal();
        }

        switch (ch)
        {
            case 'x':
                return ScanHex(2);
            case 'u':
                return ScanHex(4);
            case 'a':
                return '\u0007';
            case 'b':
                return '\b';
            case 'e':
                return '\u001B';
            case 'f':
                return '\f';
            case 'n':
                return '\n';
            case 'r':
                return '\r';
            case 't':
                return '\t';
            case 'v':
                return '\u000B';
            case 'c':
                return ScanControl();
            default:
                if (!UseOptionE() && RegexCharClass.IsWordChar(ch))
                    throw MakeException(string.Format(Strings.UnrecognizedEscape, ch.ToString()));
                return ch;
        }
    }

    /*
     * Scans X for \p{X} or \P{X}
     */
    private string ParseProperty()
    {
        if (CharsRight() < 3)
        {
            throw MakeException(Strings.IncompleteSlashP);
        }
        var ch = MoveRightGetChar();
        if (ch != '{')
        {
            throw MakeException(Strings.MalformedSlashP);
        }

        var startpos = Textpos();
        while (CharsRight() > 0)
        {
            ch = MoveRightGetChar();
            if (!(RegexCharClass.IsWordChar(ch) || ch == '-'))
            {
                MoveLeft();
                break;
            }
        }
        var capname = _pattern[startpos..Textpos()];

        if (CharsRight() == 0 || MoveRightGetChar() != '}')
            throw MakeException(Strings.IncompleteSlashP);

        return capname;
    }

    /*
     * Returns ReNode type for zero-length assertions with a \ code.
     */
    private int TypeFromCode(char ch)
    {
        return ch switch
        {
            'b' => UseOptionE() ? RegexNode.ECMABoundary : RegexNode.Boundary,
            'B' => UseOptionE() ? RegexNode.NonECMABoundary : RegexNode.Nonboundary,
            'A' => RegexNode.Beginning,
            'G' => RegexNode.Start,
            'Z' => RegexNode.EndZ,
            'z' => RegexNode.End,
            _ => RegexNode.Nothing,
        };
    }

    /*
     * Returns option bit from single-char (?cimsx) code.
     */
    private static RegexOptions OptionFromCode(char ch)
    {
        // case-insensitive
        if (ch >= 'A' && ch <= 'Z')
            ch += (char)('a' - 'A');

        return ch switch
        {
            'i' => RegexOptions.IgnoreCase,
            'r' => RegexOptions.RightToLeft,
            'm' => RegexOptions.Multiline,
            'n' => RegexOptions.ExplicitCapture,
            's' => RegexOptions.Singleline,
            'x' => RegexOptions.IgnorePatternWhitespace,
            'e' => RegexOptions.ECMAScript,
            _ => 0,
        };
    }

    /*
     * True if N option disabling '(' autocapture is on.
     */
    private bool UseOptionN()
    {
        return _options.HasAnyFlags(RegexOptions.ExplicitCapture);
    }

    /*
     * True if I option enabling case-insensitivity is on.
     */
    private bool UseOptionI()
    {
        return _options.HasAnyFlags(RegexOptions.IgnoreCase);
    }

    /*
     * True if M option altering meaning of $ and ^ is on.
     */
    private bool UseOptionM()
    {
        return _options.HasAnyFlags(RegexOptions.Multiline);
    }

    /*
     * True if S option altering meaning of . is on.
     */
    private bool UseOptionS()
    {
        return _options.HasAnyFlags(RegexOptions.Singleline);
    }

    /*
     * True if X option enabling whitespace/comment mode is on.
     */
    private bool UseOptionX()
    {
        return _options.HasAnyFlags(RegexOptions.IgnorePatternWhitespace);
    }

    /*
     * True if E option enabling ECMAScript behavior is on.
     */
    private bool UseOptionE()
    {
        return _options.HasAnyFlags(RegexOptions.ECMAScript);
    }

    internal const byte Q = 5;    // quantifier
    internal const byte S = 4;    // ordinary stopper
    internal const byte Z = 3;    // ScanBlank stopper
    internal const byte X = 2;    // whitespace
    internal const byte E = 1;    // should be escaped

    /*
     * For categorizing ASCII characters.
    */
    internal static readonly byte[] _category = new byte[] {
        // 0 1 2 3 4 5 6 7 8 9 A B C D E F 0 1 2 3 4 5 6 7 8 9 A B C D E F
           0,0,0,0,0,0,0,0,0,X,X,0,X,X,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        //   ! " # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ?
           X,0,0,Z,S,0,0,0,S,S,Q,Q,0,0,S,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,Q,
        // @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \ ] ^ _
           0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,S,S,0,S,0,
        // ' a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~
           0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,Q,S,0,0,0};

    /*
     * Returns true for those characters that terminate a string of ordinary chars.
     */
    private static bool IsSpecial(char ch)
    {
        return ch <= '|' && _category[ch] >= S;
    }

    /*
     * Returns true for those characters that terminate a string of ordinary chars.
     */
    private static bool IsStopperX(char ch)
    {
        return ch <= '|' && _category[ch] >= X;
    }

    /*
     * Returns true for those characters that begin a quantifier.
     */
    private static bool IsQuantifier(char ch)
    {
        return ch <= '{' && _category[ch] >= Q;
    }

    private bool IsTrueQuantifier()
    {
        var nChars = CharsRight();
        if (nChars == 0)
            return false;
        var startpos = Textpos();
        var ch = CharAt(startpos);
        if (ch != '{')
            return ch <= '{' && _category[ch] >= Q;
        var pos = startpos;
        while (--nChars > 0 && (ch = CharAt(++pos)) >= '0' && ch <= '9') { }
        if (nChars == 0 || pos - startpos == 1)
            return false;
        if (ch == '}')
            return true;
        if (ch != ',')
            return false;
        while (--nChars > 0 && (ch = CharAt(++pos)) >= '0' && ch <= '9') { }
        return nChars > 0 && ch == '}';
    }

    /*
     * Returns true for whitespace.
     */
    private static bool IsSpace(char ch)
    {
        return ch <= ' ' && _category[ch] == X;
    }

    /*
     * Add a string to the last concatenate.
     */
    private void AddConcatenate(int pos, int cch)
    {
        RegexNode node;

        if (cch == 0)
            return;

        if (cch > 1)
        {
            var str = _pattern.Substring(pos, cch);

            if (UseOptionI())
            {
                // We do the ToLower character by character for consistency.  With surrogate chars, doing
                // a ToLower on the entire string could actually change the surrogate pair.  This is more correct
                // linguistically, but since Regex doesn't support surrogates, it's more important to be
                // consistent.
                var sb = new StringBuilder(str.Length);
                for (var i = 0; i < str.Length; i++)
                    sb.Append(_culture.TextInfo.ToLower(str[i]));
                str = sb.ToString();
            }

            node = new RegexNode(RegexNode.Multi, _options, str);
        }
        else
        {
            var ch = _pattern[pos];

            if (UseOptionI())
                ch = _culture.TextInfo.ToLower(ch);

            node = new RegexNode(RegexNode.One, _options, ch);
        }

        _concatenation.AddChild(node);
    }

    /*
     * Push the parser state (in response to an open paren)
     */
    private void PushGroup()
    {
        _group._next = _stack;
        _alternation._next = _group;
        _concatenation._next = _alternation;
        _stack = _concatenation;
    }

    /*
     * Remember the pushed state (in response to a ')')
     */
    private void PopGroup()
    {
        _concatenation = _stack;
        _alternation = _concatenation._next;
        _group = _alternation._next;
        _stack = _group._next;

        // The first () inside a Testgroup group goes directly to the group
        if (_group.Type() == RegexNode.Testgroup && _group.ChildCount() == 0)
        {
            if (_unit == null)
                throw MakeException(Strings.IllegalCondition);

            _group.AddChild(_unit);
            _unit = null;
        }
    }

    /*
     * True if the group stack is empty.
     */
    private bool EmptyStack()
    {
        return _stack == null;
    }

    /*
     * Start a new round for the parser state (in response to an open paren or string start)
     */
    private void StartGroup(RegexNode openGroup)
    {
        _group = openGroup;
        _alternation = new RegexNode(RegexNode.Alternate, _options);
        _concatenation = new RegexNode(RegexNode.Concatenate, _options);
    }

    /*
     * Finish the current concatenation (in response to a |)
     */
    private void AddAlternate()
    {
        // The | parts inside a Testgroup group go directly to the group

        if (_group.Type() == RegexNode.Testgroup || _group.Type() == RegexNode.Testref)
        {
            _group.AddChild(_concatenation.ReverseLeft());
        }
        else
        {
            _alternation.AddChild(_concatenation.ReverseLeft());
        }

        _concatenation = new RegexNode(RegexNode.Concatenate, _options);
    }

    /*
     * Finish the current quantifiable (when a quantifier is not found or is not possible)
     */
    private void AddConcatenate()
    {
        // The first (| inside a Testgroup group goes directly to the group

        _concatenation.AddChild(_unit);
        _unit = null;
    }

    /*
     * Finish the current quantifiable (when a quantifier is found)
     */
    private void AddConcatenate(bool lazy, int min, int max)
    {
        _concatenation.AddChild(_unit.MakeQuantifier(lazy, min, max));
        _unit = null;
    }

    /*
     * Returns the current unit
     */
    private RegexNode Unit()
    {
        return _unit;
    }

    /*
     * Sets the current unit to a single char node
     */
    private void AddUnitOne(char ch)
    {
        if (UseOptionI())
            ch = _culture.TextInfo.ToLower(ch);

        _unit = new RegexNode(RegexNode.One, _options, ch);
    }

    /*
     * Sets the current unit to a single inverse-char node
     */
    private void AddUnitNotone(char ch)
    {
        if (UseOptionI())
            ch = _culture.TextInfo.ToLower(ch);

        _unit = new RegexNode(RegexNode.Notone, _options, ch);
    }

    /*
     * Sets the current unit to a single set node
     */
    private void AddUnitSet(string cc)
    {
        _unit = new RegexNode(RegexNode.Set, _options, cc);
    }

    /*
     * Sets the current unit to a subtree
     */
    private void AddUnitNode(RegexNode node)
    {
        _unit = node;
    }

    /*
     * Sets the current unit to an assertion of the specified type
     */
    private void AddUnitType(int type)
    {
        _unit = new RegexNode(type, _options);
    }

    /*
     * Finish the current group (in response to a ')' or end)
     */
    private void AddGroup()
    {
        if (_group.Type() == RegexNode.Testgroup || _group.Type() == RegexNode.Testref)
        {
            _group.AddChild(_concatenation.ReverseLeft());

            if ((_group.Type() == RegexNode.Testref && _group.ChildCount() > 2) || _group.ChildCount() > 3)
                throw MakeException(Strings.TooManyAlternates);
        }
        else
        {
            _alternation.AddChild(_concatenation.ReverseLeft());
            _group.AddChild(_alternation);
        }

        _unit = _group;
    }

    /*
     * Saves options on a stack.
     */
    private void PushOptions()
    {
        _optionsStack.Add(_options);
    }

    /*
     * Recalls options from the stack.
     */
    private void PopOptions()
    {
        _options = _optionsStack[^1];
        _optionsStack.RemoveAt(_optionsStack.Count - 1);
    }

    /*
     * Pops the option stack, but keeps the current options unchanged.
     */
    private void PopKeepOptions()
    {
        _optionsStack.RemoveAt(_optionsStack.Count - 1);
    }

    /*
     * Fills in an ArgumentException
     */
    private ArgumentException MakeException(string message)
    {
        return new ArgumentException(string.Format(Strings.MakeException, _pattern, message));
    }

    /*
     * Returns the current parsing position.
     */
    private int Textpos()
    {
        return _currentPos;
    }

    /*
     * Zaps to a specific parsing position.
     */
    private void Textto(int pos)
    {
        _currentPos = pos;
    }

    /*
     * Returns the char at the right of the current parsing position and advances to the right.
     */
    private char MoveRightGetChar()
    {
        return _pattern[_currentPos++];
    }

    /*
     * Moves the current position to the right.
     */
    private void MoveRight()
    {
        MoveRight(1);
    }

    private void MoveRight(int i)
    {
        _currentPos += i;
    }

    /*
     * Moves the current parsing position one to the left.
     */
    private void MoveLeft()
    {
        --_currentPos;
    }

    /*
     * Returns the char left of the current parsing position.
     */
    private char CharAt(int i)
    {
        return _pattern[i];
    }

    /*
     * Returns the char right of the current parsing position.
     */
    private char RightChar()
    {
        return _pattern[_currentPos];
    }

    /*
     * Returns the char i chars right of the current parsing position.
     */
    private char RightChar(int i)
    {
        return _pattern[_currentPos + i];
    }

    /*
     * Number of characters to the right of the current parsing position.
     */
    private int CharsRight()
    {
        return _pattern.Length - _currentPos;
    }
}