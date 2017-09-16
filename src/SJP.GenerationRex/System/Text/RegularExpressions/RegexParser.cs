using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexParser
    {
        internal static readonly byte[] _category = new byte[128]
        {
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       2,
       2,
       0,
       2,
       2,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       2,
       0,
       0,
       3,
       4,
       0,
       0,
       0,
       4,
       4,
       5,
       5,
       0,
       0,
       4,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       5,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       4,
       4,
       0,
       4,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       0,
       5,
       4,
       0,
       0,
       0
        };
        internal const int MaxValueDiv10 = 214748364;
        internal const int MaxValueMod10 = 7;
        internal const byte Q = 5;
        internal const byte S = 4;
        internal const byte Z = 3;
        internal const byte X = 2;
        internal const byte E = 1;
        internal RegexNode _stack;
        internal RegexNode _group;
        internal RegexNode _alternation;
        internal RegexNode _concatenation;
        internal RegexNode _unit;
        internal string _pattern;
        internal int _currentPos;
        internal CultureInfo _culture;
        internal int _autocap;
        internal int _capcount;
        internal int _captop;
        internal int _capsize;
        internal Hashtable _caps;
        internal Hashtable _capnames;
        internal int[] _capnumlist;
        internal List<string> _capnamelist;
        internal RegexOptions _options;
        internal List<RegexOptions> _optionsStack;
        internal bool _ignoreNextParen;

        internal static RegexTree Parse(string re, RegexOptions op)
        {
            var regexParser = new RegexParser((op & RegexOptions.CultureInvariant) != RegexOptions.None ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
            regexParser._options = op;
            regexParser.SetPattern(re);
            regexParser.CountCaptures();
            regexParser.Reset(op);
            RegexNode root = regexParser.ScanRegex();
            string[] capslist = regexParser._capnamelist != null ? regexParser._capnamelist.ToArray() : null;
            return new RegexTree(root, regexParser._caps, regexParser._capnumlist, regexParser._captop, regexParser._capnames, capslist, op);
        }

        internal static RegexReplacement ParseReplacement(string rep, Hashtable caps, int capsize, Hashtable capnames, RegexOptions op)
        {
            var regexParser = new RegexParser((op & RegexOptions.CultureInvariant) != RegexOptions.None ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
            regexParser._options = op;
            regexParser.NoteCaptures(caps, capsize, capnames);
            regexParser.SetPattern(rep);
            RegexNode concat = regexParser.ScanReplacement();
            return new RegexReplacement(rep, concat, caps);
        }

        internal static string Escape(string input)
        {
            for (int count = 0; count < input.Length; ++count)
            {
                if (RegexParser.IsMetachar(input[count]))
                {
                    var stringBuilder = new StringBuilder();
                    char ch = input[count];
                    stringBuilder.Append(input, 0, count);
                    do
                    {
                        stringBuilder.Append('\\');
                        switch (ch)
                        {
                            case '\t':
                                ch = 't';
                                break;
                            case '\n':
                                ch = 'n';
                                break;
                            case '\f':
                                ch = 'f';
                                break;
                            case '\r':
                                ch = 'r';
                                break;
                        }
                        stringBuilder.Append(ch);
                        ++count;
                        int startIndex = count;
                        for (; count < input.Length; ++count)
                        {
                            ch = input[count];
                            if (RegexParser.IsMetachar(ch))
                                break;
                        }
                        stringBuilder.Append(input, startIndex, count - startIndex);
                    }
                    while (count < input.Length);
                    return stringBuilder.ToString();
                }
            }
            return input;
        }

        internal static string Unescape(string input)
        {
            for (int count = 0; count < input.Length; ++count)
            {
                if (input[count] == 92)
                {
                    var stringBuilder = new StringBuilder();
                    var regexParser = new RegexParser(CultureInfo.InvariantCulture);
                    regexParser.SetPattern(input);
                    stringBuilder.Append(input, 0, count);
                    do
                    {
                        int pos = count + 1;
                        regexParser.Textto(pos);
                        if (pos < input.Length)
                            stringBuilder.Append(regexParser.ScanCharEscape());
                        count = regexParser.Textpos();
                        int startIndex = count;
                        while (count < input.Length && input[count] != 92)
                            ++count;
                        stringBuilder.Append(input, startIndex, count - startIndex);
                    }
                    while (count < input.Length);
                    return stringBuilder.ToString();
                }
            }
            return input;
        }

        private RegexParser(CultureInfo culture)
        {
            _culture = culture;
            _optionsStack = new List<RegexOptions>();
            _caps = new Hashtable();
        }

        internal void SetPattern(string Re)
        {
            if (Re == null)
                Re = string.Empty;
            _pattern = Re;
            _currentPos = 0;
        }

        internal void Reset(RegexOptions topopts)
        {
            _currentPos = 0;
            _autocap = 1;
            _ignoreNextParen = false;
            if (_optionsStack.Count > 0)
                _optionsStack.RemoveRange(0, _optionsStack.Count - 1);
            _options = topopts;
            _stack = null;
        }

        internal RegexNode ScanRegex()
        {
            bool flag1 = false;
            StartGroup(new RegexNode(28, _options, 0, -1));
            label_57:
            while (CharsRight() > 0)
            {
                bool flag2 = flag1;
                flag1 = false;
                ScanBlank();
                int pos = Textpos();
                if (UseOptionX())
                {
                    char ch;
                    while (CharsRight() > 0 && (!RegexParser.IsStopperX(ch = RightChar()) || ch == 123 && !IsTrueQuantifier()))
                        MoveRight();
                }
                else
                {
                    char ch;
                    while (CharsRight() > 0 && (!RegexParser.IsSpecial(ch = RightChar()) || ch == 123 && !IsTrueQuantifier()))
                        MoveRight();
                }
                int num1 = Textpos();
                ScanBlank();
                char ch1;
                if (CharsRight() == 0)
                    ch1 = '!';
                else if (RegexParser.IsSpecial(ch1 = RightChar()))
                {
                    flag1 = RegexParser.IsQuantifier(ch1);
                    MoveRight();
                }
                else
                    ch1 = ' ';
                if (pos < num1)
                {
                    int cch = num1 - pos - (flag1 ? 1 : 0);
                    flag2 = false;
                    if (cch > 0)
                        AddConcatenate(pos, cch, false);
                    if (flag1)
                        AddUnitOne(CharAt(num1 - 1));
                }
                switch (ch1)
                {
                    case '[':
                        AddUnitSet(ScanCharClass(UseOptionI()).ToStringClass());
                        break;
                    case '\\':
                        AddUnitNode(ScanBackslash());
                        break;
                    case '^':
                        AddUnitType(UseOptionM() ? 14 : 18);
                        break;
                    case '{':
                    case '*':
                    case '+':
                    case '?':
                        if (Unit() == null)
                        {
                            string message;
                            if (!flag2)
                                message = Strings.GetString("QuantifyAfterNothing");
                            else
                                message = Strings.GetString("NestedQuantify", (object)ch1.ToString());
                            throw MakeException(message);
                        }
                        MoveLeft();
                        break;
                    case '|':
                        AddAlternate();
                        continue;
                    case ' ':
                        continue;
                    case '!':
                        goto label_58;
                    case '$':
                        AddUnitType(UseOptionM() ? 15 : 20);
                        break;
                    case '(':
                        PushOptions();
                        RegexNode openGroup;
                        if ((openGroup = ScanGroupOpen()) == null)
                        {
                            PopKeepOptions();
                            continue;
                        }
                        PushGroup();
                        StartGroup(openGroup);
                        continue;
                    case ')':
                        if (EmptyStack())
                            throw MakeException(Strings.GetString("TooManyParens"));
                        AddGroup();
                        PopGroup();
                        PopOptions();
                        if (Unit() != null)
                            break;
                        continue;
                    case '.':
                        if (UseOptionS())
                        {
                            AddUnitSet("\0\x0001\0\0");
                            break;
                        }
                        AddUnitNotone('\n');
                        break;
                    default:
                        throw MakeException(Strings.GetString("InternalError"));
                }
                ScanBlank();
                if (CharsRight() == 0 || !(flag1 = IsTrueQuantifier()))
                {
                    AddConcatenate();
                }
                else
                {
                    char ch2 = MoveRightGetChar();
                    while (Unit() != null)
                    {
                        int min;
                        int max;
                        switch (ch2)
                        {
                            case '*':
                                min = 0;
                                max = int.MaxValue;
                                break;
                            case '+':
                                min = 1;
                                max = int.MaxValue;
                                break;
                            case '?':
                                min = 0;
                                max = 1;
                                break;
                            case '{':
                                int num2 = Textpos();
                                max = min = ScanDecimal();
                                if (num2 < Textpos() && CharsRight() > 0 && RightChar() == 44)
                                {
                                    MoveRight();
                                    max = CharsRight() == 0 || RightChar() == 125 ? int.MaxValue : ScanDecimal();
                                }
                                if (num2 == Textpos() || CharsRight() == 0 || MoveRightGetChar() != 125)
                                {
                                    AddConcatenate();
                                    Textto(num2 - 1);
                                    goto label_57;
                                }
                                else
                                    break;
                            default:
                                throw MakeException(Strings.GetString("InternalError"));
                        }
                        ScanBlank();
                        bool lazy;
                        if (CharsRight() == 0 || RightChar() != 63)
                        {
                            lazy = false;
                        }
                        else
                        {
                            MoveRight();
                            lazy = true;
                        }
                        if (min > max)
                            throw MakeException(Strings.GetString("IllegalRange"));
                        AddConcatenate(lazy, min, max);
                    }
                }
            }
            label_58:
            if (!EmptyStack())
                throw MakeException(Strings.GetString("NotEnoughParens"));
            AddGroup();
            return Unit();
        }

        internal RegexNode ScanReplacement()
        {
            _concatenation = new RegexNode(25, _options);
            while (true)
            {
                int num;
                do
                {
                    num = CharsRight();
                    if (num != 0)
                    {
                        int pos = Textpos();
                        for (; num > 0 && RightChar() != 36; --num)
                            MoveRight();
                        AddConcatenate(pos, Textpos() - pos, true);
                    }
                    else
                        goto label_9;
                }
                while (num <= 0);
                if (MoveRightGetChar() == 36)
                    AddUnitNode(ScanDollar());
                AddConcatenate();
            }
            label_9:
            return _concatenation;
        }

        internal RegexCharClass ScanCharClass(bool caseInsensitive)
        {
            return ScanCharClass(caseInsensitive, false);
        }

        internal RegexCharClass ScanCharClass(bool caseInsensitive, bool scanOnly)
        {
            char ch1 = char.MinValue;
            bool flag1 = false;
            bool flag2 = true;
            bool flag3 = false;
            RegexCharClass regexCharClass = scanOnly ? null : new RegexCharClass();
            if (CharsRight() > 0 && RightChar() == 94)
            {
                MoveRight();
                if (!scanOnly)
                    regexCharClass.Negate = true;
            }
            while (CharsRight() > 0)
            {
                bool flag4 = false;
                char ch2 = MoveRightGetChar();
                if (ch2 == 93)
                {
                    if (!flag2)
                    {
                        flag3 = true;
                        break;
                    }
                }
                else if (ch2 == 92 && CharsRight() > 0)
                {
                    char ch3;
                    switch (ch3 = MoveRightGetChar())
                    {
                        case 'p':
                        case 'P':
                            if (!scanOnly)
                            {
                                if (flag1)
                                    throw MakeException(Strings.GetString("BadClassInCharRange", (object)ch3.ToString()));
                                regexCharClass.AddCategoryFromName(ParseProperty(), ch3 != 112, caseInsensitive, _pattern);
                                goto label_48;
                            }
                            else
                            {
                                ParseProperty();
                                goto label_48;
                            }
                        case 's':
                        case 'S':
                            if (!scanOnly)
                            {
                                if (flag1)
                                    throw MakeException(Strings.GetString("BadClassInCharRange", (object)ch3.ToString()));
                                regexCharClass.AddSpace(UseOptionE(), ch3 == 83);
                                goto label_48;
                            }
                            else
                                goto label_48;
                        case 'w':
                        case 'W':
                            if (!scanOnly)
                            {
                                if (flag1)
                                    throw MakeException(Strings.GetString("BadClassInCharRange", (object)ch3.ToString()));
                                regexCharClass.AddWord(UseOptionE(), ch3 == 87);
                                goto label_48;
                            }
                            else
                                goto label_48;
                        case 'd':
                        case 'D':
                            if (!scanOnly)
                            {
                                if (flag1)
                                    throw MakeException(Strings.GetString("BadClassInCharRange", (object)ch3.ToString()));
                                regexCharClass.AddDigit(UseOptionE(), ch3 == 68, _pattern);
                                goto label_48;
                            }
                            else
                                goto label_48;
                        case '-':
                            if (!scanOnly)
                            {
                                regexCharClass.AddRange(ch3, ch3);
                                goto label_48;
                            }
                            else
                                goto label_48;
                        default:
                            MoveLeft();
                            ch2 = ScanCharEscape();
                            flag4 = true;
                            break;
                    }
                }
                else if (ch2 == 91 && CharsRight() > 0 && (RightChar() == 58 && !flag1))
                {
                    int pos = Textpos();
                    MoveRight();
                    ScanCapname();
                    if (CharsRight() < 2 || MoveRightGetChar() != 58 || MoveRightGetChar() != 93)
                        Textto(pos);
                }
                if (flag1)
                {
                    flag1 = false;
                    if (!scanOnly)
                    {
                        if (ch2 == 91 && !flag4 && !flag2)
                        {
                            regexCharClass.AddChar(ch1);
                            regexCharClass.AddSubtraction(ScanCharClass(caseInsensitive, false));
                            if (CharsRight() > 0 && RightChar() != 93)
                                throw MakeException(Strings.GetString("SubtractionMustBeLast"));
                        }
                        else
                        {
                            if (ch1 > ch2)
                                throw MakeException(Strings.GetString("ReversedCharRange"));
                            regexCharClass.AddRange(ch1, ch2);
                        }
                    }
                }
                else if (CharsRight() >= 2 && RightChar() == 45 && RightChar(1) != 93)
                {
                    ch1 = ch2;
                    flag1 = true;
                    MoveRight();
                }
                else if (CharsRight() >= 1 && ch2 == 45 && (!flag4 && RightChar() == 91) && !flag2)
                {
                    if (!scanOnly)
                    {
                        MoveRight(1);
                        regexCharClass.AddSubtraction(ScanCharClass(caseInsensitive, false));
                        if (CharsRight() > 0 && RightChar() != 93)
                            throw MakeException(Strings.GetString("SubtractionMustBeLast"));
                    }
                    else
                    {
                        MoveRight(1);
                        ScanCharClass(caseInsensitive, true);
                    }
                }
                else if (!scanOnly)
                    regexCharClass.AddRange(ch2, ch2);
                label_48:
                flag2 = false;
            }
            if (!flag3)
                throw MakeException(Strings.GetString("UnterminatedBracket"));
            if (!scanOnly && caseInsensitive)
                regexCharClass.AddLowercase(_culture);
            return regexCharClass;
        }

        internal RegexNode ScanGroupOpen()
        {
            char minValue = char.MinValue;
            char ch1 = '>';
            if (CharsRight() == 0 || RightChar() != 63 || RightChar() == 63 && CharsRight() > 1 && RightChar(1) == 41)
            {
                if (!UseOptionN() && !_ignoreNextParen)
                    return new RegexNode(28, _options, _autocap++, -1);
                _ignoreNextParen = false;
                return new RegexNode(29, _options);
            }
            MoveRight();
            if (CharsRight() != 0)
            {
                int type;
                switch (minValue = MoveRightGetChar())
                {
                    case '!':
                        _options &= ~RegexOptions.RightToLeft;
                        type = 31;
                        break;
                    case '\'':
                        ch1 = '\'';
                        goto case '<';
                    case '(':
                        int num1 = Textpos();
                        if (CharsRight() > 0)
                        {
                            char ch2 = RightChar();
                            if (ch2 >= 48 && ch2 <= 57)
                            {
                                int num2 = ScanDecimal();
                                if (CharsRight() > 0 && MoveRightGetChar() == 41)
                                {
                                    if (IsCaptureSlot(num2))
                                        return new RegexNode(33, _options, num2);
                                    throw MakeException(Strings.GetString("UndefinedReference", (object)num2.ToString(CultureInfo.CurrentCulture)));
                                }
                                throw MakeException(Strings.GetString("MalformedReference", (object)num2.ToString(CultureInfo.CurrentCulture)));
                            }
                            if (RegexCharClass.IsWordChar(ch2))
                            {
                                string capname = ScanCapname();
                                if (IsCaptureName(capname) && CharsRight() > 0 && MoveRightGetChar() == 41)
                                    return new RegexNode(33, _options, CaptureSlotFromName(capname));
                            }
                        }
                        type = 34;
                        Textto(num1 - 1);
                        _ignoreNextParen = true;
                        int num3 = CharsRight();
                        if (num3 >= 3 && RightChar(1) == 63)
                        {
                            char ch2 = RightChar(2);
                            switch (ch2)
                            {
                                case '#':
                                    throw MakeException(Strings.GetString("AlternationCantHaveComment"));
                                case '\'':
                                    throw MakeException(Strings.GetString("AlternationCantCapture"));
                                default:
                                    if (num3 >= 4 && ch2 == 60 && (RightChar(3) != 33 && RightChar(3) != 61))
                                        throw MakeException(Strings.GetString("AlternationCantCapture"));
                                    break;
                            }
                        }
                        break;
                    case ':':
                        type = 29;
                        break;
                    case '<':
                        if (CharsRight() != 0)
                        {
                            char ch2;
                            switch (ch2 = MoveRightGetChar())
                            {
                                case '!':
                                    if (ch1 != 39)
                                    {
                                        _options |= RegexOptions.RightToLeft;
                                        type = 31;
                                        break;
                                    }
                                    goto label_67;
                                case '=':
                                    if (ch1 != 39)
                                    {
                                        _options |= RegexOptions.RightToLeft;
                                        type = 30;
                                        break;
                                    }
                                    goto label_67;
                                default:
                                    MoveLeft();
                                    int num2 = -1;
                                    int num4 = -1;
                                    bool flag = false;
                                    if (ch2 >= 48 && ch2 <= 57)
                                    {
                                        num2 = ScanDecimal();
                                        if (!IsCaptureSlot(num2))
                                            num2 = -1;
                                        if (CharsRight() > 0 && RightChar() != ch1 && RightChar() != 45)
                                            throw MakeException(Strings.GetString("InvalidGroupName"));
                                        if (num2 == 0)
                                            throw MakeException(Strings.GetString("CapnumNotZero"));
                                    }
                                    else if (RegexCharClass.IsWordChar(ch2))
                                    {
                                        string capname = ScanCapname();
                                        if (IsCaptureName(capname))
                                            num2 = CaptureSlotFromName(capname);
                                        if (CharsRight() > 0 && RightChar() != ch1 && RightChar() != 45)
                                            throw MakeException(Strings.GetString("InvalidGroupName"));
                                    }
                                    else
                                    {
                                        if (ch2 != 45)
                                            throw MakeException(Strings.GetString("InvalidGroupName"));
                                        flag = true;
                                    }
                                    if ((num2 != -1 || flag) && (CharsRight() > 0 && RightChar() == 45))
                                    {
                                        MoveRight();
                                        char ch3 = RightChar();
                                        if (ch3 >= 48 && ch3 <= 57)
                                        {
                                            num4 = ScanDecimal();
                                            if (!IsCaptureSlot(num4))
                                                throw MakeException(Strings.GetString("UndefinedBackref", (object)num4));
                                            if (CharsRight() > 0 && RightChar() != ch1)
                                                throw MakeException(Strings.GetString("InvalidGroupName"));
                                        }
                                        else
                                        {
                                            if (!RegexCharClass.IsWordChar(ch3))
                                                throw MakeException(Strings.GetString("InvalidGroupName"));
                                            string capname = ScanCapname();
                                            if (IsCaptureName(capname))
                                            {
                                                num4 = CaptureSlotFromName(capname);
                                                if (CharsRight() > 0 && RightChar() != ch1)
                                                    throw MakeException(Strings.GetString("InvalidGroupName"));
                                            }
                                            else
                                                throw MakeException(Strings.GetString("UndefinedNameRef", (object)capname));
                                        }
                                    }
                                    if ((num2 != -1 || num4 != -1) && (CharsRight() > 0 && MoveRightGetChar() == ch1))
                                        return new RegexNode(28, _options, num2, num4);
                                    goto label_67;
                            }
                        }
                        else
                            goto label_67;
                        break;
                    case '=':
                        _options &= ~RegexOptions.RightToLeft;
                        type = 30;
                        break;
                    case '>':
                        type = 32;
                        break;
                    default:
                        MoveLeft();
                        type = 29;
                        ScanOptions();
                        if (CharsRight() != 0)
                        {
                            char ch2;
                            if ((ch2 = MoveRightGetChar()) == 41)
                                return null;
                            if (ch2 != 58)
                                goto label_67;
                            else
                                break;
                        }
                        else
                            goto label_67;
                }
                return new RegexNode(type, _options);
            }
            label_67:
            throw MakeException(Strings.GetString("UnrecognizedGrouping"));
        }

        internal void ScanBlank()
        {
            if (UseOptionX())
            {
                label_2:
                while (true)
                {
                    while (CharsRight() <= 0 || !RegexParser.IsSpace(RightChar()))
                    {
                        if (CharsRight() == 0)
                            return;
                        if (RightChar() == 35)
                        {
                            while (true)
                            {
                                if (CharsRight() > 0 && RightChar() != 10)
                                    MoveRight();
                                else
                                    goto label_2;
                            }
                        }
                        else
                        {
                            if (CharsRight() < 3 || RightChar(2) != 35 || (RightChar(1) != 63 || RightChar() != 40))
                                return;
                            while (CharsRight() > 0 && RightChar() != 41)
                                MoveRight();
                            if (CharsRight() == 0)
                                throw MakeException(Strings.GetString("UnterminatedComment"));
                            MoveRight();
                        }
                    }
                    MoveRight();
                }
            }
            else
            {
                while (CharsRight() >= 3 && RightChar(2) == 35 && (RightChar(1) == 63 && RightChar() == 40))
                {
                    while (CharsRight() > 0 && RightChar() != 41)
                        MoveRight();
                    if (CharsRight() == 0)
                        throw MakeException(Strings.GetString("UnterminatedComment"));
                    MoveRight();
                }
            }
        }

        internal RegexNode ScanBackslash()
        {
            if (CharsRight() == 0)
                throw MakeException(Strings.GetString("IllegalEndEscape"));
            char ch;
            switch (ch = RightChar())
            {
                case 's':
                    MoveRight();
                    if (UseOptionE())
                        return new RegexNode(11, _options, "\0\x0004\0\t\x000E !");
                    return new RegexNode(11, _options, RegexCharClass.SpaceClass);
                case 'w':
                    MoveRight();
                    if (UseOptionE())
                        return new RegexNode(11, _options, "\0\n\00:A[_`a{İı");
                    return new RegexNode(11, _options, RegexCharClass.WordClass);
                case 'z':
                case 'b':
                case 'Z':
                case 'A':
                case 'B':
                case 'G':
                    MoveRight();
                    return new RegexNode(TypeFromCode(ch), _options);
                case 'd':
                    MoveRight();
                    if (UseOptionE())
                        return new RegexNode(11, _options, "\0\x0002\00:");
                    return new RegexNode(11, _options, RegexCharClass.DigitClass);
                case 'p':
                case 'P':
                    MoveRight();
                    var regexCharClass = new RegexCharClass();
                    regexCharClass.AddCategoryFromName(ParseProperty(), ch != 112, UseOptionI(), _pattern);
                    if (UseOptionI())
                        regexCharClass.AddLowercase(_culture);
                    return new RegexNode(11, _options, regexCharClass.ToStringClass());
                case 'S':
                    MoveRight();
                    if (UseOptionE())
                        return new RegexNode(11, _options, "\x0001\x0004\0\t\x000E !");
                    return new RegexNode(11, _options, RegexCharClass.NotSpaceClass);
                case 'W':
                    MoveRight();
                    if (UseOptionE())
                        return new RegexNode(11, _options, "\x0001\n\00:A[_`a{İı");
                    return new RegexNode(11, _options, RegexCharClass.NotWordClass);
                case 'D':
                    MoveRight();
                    if (UseOptionE())
                        return new RegexNode(11, _options, "\x0001\x0002\00:");
                    return new RegexNode(11, _options, RegexCharClass.NotDigitClass);
                default:
                    return ScanBasicBackslash();
            }
        }

        internal RegexNode ScanBasicBackslash()
        {
            if (CharsRight() == 0)
                throw MakeException(Strings.GetString("IllegalEndEscape"));
            bool flag = false;
            char ch1 = char.MinValue;
            int pos = Textpos();
            char ch2 = RightChar();
            switch (ch2)
            {
                case 'k':
                    if (CharsRight() >= 2)
                    {
                        MoveRight();
                        char ch3 = MoveRightGetChar();
                        switch (ch3)
                        {
                            case '<':
                            case '\'':
                                flag = true;
                                ch1 = ch3 == 39 ? '\'' : '>';
                                break;
                        }
                    }
                    if (!flag || CharsRight() <= 0)
                        throw MakeException(Strings.GetString("MalformedNameRef"));
                    ch2 = RightChar();
                    break;
                case '<':
                case '\'':
                    if (CharsRight() > 1)
                    {
                        flag = true;
                        ch1 = ch2 == 39 ? '\'' : '>';
                        MoveRight();
                        ch2 = RightChar();
                        break;
                    }
                    break;
            }
            if (flag && ch2 >= 48 && ch2 <= 57)
            {
                int num = ScanDecimal();
                if (CharsRight() > 0 && MoveRightGetChar() == ch1)
                {
                    if (IsCaptureSlot(num))
                        return new RegexNode(13, _options, num);
                    throw MakeException(Strings.GetString("UndefinedBackref", (object)num.ToString(CultureInfo.CurrentCulture)));
                }
            }
            else if (!flag && ch2 >= 49 && ch2 <= 57)
            {
                if (UseOptionE())
                {
                    int m = -1;
                    int i = ch2 - 48;
                    int num = Textpos() - 1;
                    char ch3;
                    for (; i <= _captop; i = i * 10 + (ch3 - 48))
                    {
                        if (IsCaptureSlot(i) && (_caps == null || (int)_caps[i] < num))
                            m = i;
                        MoveRight();
                        if (CharsRight() == 0 || (ch3 = RightChar()) < 48 || ch3 > 57)
                            break;
                    }
                    if (m >= 0)
                        return new RegexNode(13, _options, m);
                }
                else
                {
                    int num = ScanDecimal();
                    if (IsCaptureSlot(num))
                        return new RegexNode(13, _options, num);
                    if (num <= 9)
                        throw MakeException(Strings.GetString("UndefinedBackref", (object)num.ToString(CultureInfo.CurrentCulture)));
                }
            }
            else if (flag && RegexCharClass.IsWordChar(ch2))
            {
                string capname = ScanCapname();
                if (CharsRight() > 0 && MoveRightGetChar() == ch1)
                {
                    if (IsCaptureName(capname))
                        return new RegexNode(13, _options, CaptureSlotFromName(capname));
                    throw MakeException(Strings.GetString("UndefinedNameRef", (object)capname));
                }
            }
            Textto(pos);
            char ch4 = ScanCharEscape();
            if (UseOptionI())
                ch4 = char.ToLower(ch4, _culture);
            return new RegexNode(9, _options, ch4);
        }

        internal RegexNode ScanDollar()
        {
            if (CharsRight() == 0)
                return new RegexNode(9, _options, '$');
            char ch1 = RightChar();
            int pos1 = Textpos();
            int pos2 = pos1;
            bool flag;
            if (ch1 == 123 && CharsRight() > 1)
            {
                flag = true;
                MoveRight();
                ch1 = RightChar();
            }
            else
                flag = false;
            if (ch1 >= 48 && ch1 <= 57)
            {
                if (!flag && UseOptionE())
                {
                    int m = -1;
                    int i = ch1 - 48;
                    MoveRight();
                    if (IsCaptureSlot(i))
                    {
                        m = i;
                        pos2 = Textpos();
                    }
                    char ch2;
                    while (CharsRight() > 0 && (ch2 = RightChar()) >= 48 && ch2 <= 57)
                    {
                        int num = ch2 - 48;
                        if (i > 214748364 || i == 214748364 && num > 7)
                            throw MakeException(Strings.GetString("CaptureGroupOutOfRange"));
                        i = i * 10 + num;
                        MoveRight();
                        if (IsCaptureSlot(i))
                        {
                            m = i;
                            pos2 = Textpos();
                        }
                    }
                    Textto(pos2);
                    if (m >= 0)
                        return new RegexNode(13, _options, m);
                }
                else
                {
                    int num = ScanDecimal();
                    if ((!flag || CharsRight() > 0 && MoveRightGetChar() == 125) && IsCaptureSlot(num))
                        return new RegexNode(13, _options, num);
                }
            }
            else if (flag && RegexCharClass.IsWordChar(ch1))
            {
                string capname = ScanCapname();
                if (CharsRight() > 0 && MoveRightGetChar() == 125 && IsCaptureName(capname))
                    return new RegexNode(13, _options, CaptureSlotFromName(capname));
            }
            else if (!flag)
            {
                int m = 1;
                switch (ch1)
                {
                    case '$':
                        MoveRight();
                        return new RegexNode(9, _options, '$');
                    case '&':
                        m = 0;
                        break;
                    case '\'':
                        m = -2;
                        break;
                    case '+':
                        m = -3;
                        break;
                    case '_':
                        m = -4;
                        break;
                    case '`':
                        m = -1;
                        break;
                }
                if (m != 1)
                {
                    MoveRight();
                    return new RegexNode(13, _options, m);
                }
            }
            Textto(pos1);
            return new RegexNode(9, _options, '$');
        }

        internal string ScanCapname()
        {
            int startIndex = Textpos();
            while (CharsRight() > 0)
            {
                if (!RegexCharClass.IsWordChar(MoveRightGetChar()))
                {
                    MoveLeft();
                    break;
                }
            }
            return _pattern.Substring(startIndex, Textpos() - startIndex);
        }

        internal char ScanOctal()
        {
            int num1 = 3;
            if (num1 > CharsRight())
                num1 = CharsRight();
            int num2 = 0;
            int num3;
            for (; num1 > 0 && (uint)(num3 = RightChar() - 48) <= 7U; --num1)
            {
                MoveRight();
                num2 = num2 * 8 + num3;
                if (UseOptionE() && num2 >= 32)
                    break;
            }
            return (char)(num2 & byte.MaxValue);
        }

        internal int ScanDecimal()
        {
            int num1 = 0;
            int num2;
            while (CharsRight() > 0 && (uint)(num2 = (ushort)((uint)RightChar() - 48U)) <= 9U)
            {
                MoveRight();
                if (num1 > 214748364 || num1 == 214748364 && num2 > 7)
                    throw MakeException(Strings.GetString("CaptureGroupOutOfRange"));
                num1 = num1 * 10 + num2;
            }
            return num1;
        }

        internal char ScanHex(int c)
        {
            int num1 = 0;
            if (CharsRight() >= c)
            {
                int num2;
                for (; c > 0 && (num2 = RegexParser.HexDigit(MoveRightGetChar())) >= 0; --c)
                    num1 = num1 * 16 + num2;
            }
            if (c > 0)
                throw MakeException(Strings.GetString("TooFewHex"));
            return (char)num1;
        }

        internal static int HexDigit(char ch)
        {
            int num1;
            if ((uint)(num1 = ch - 48) <= 9U)
                return num1;
            int num2;
            if ((uint)(num2 = ch - 97) <= 5U)
                return num2 + 10;
            int num3;
            if ((uint)(num3 = ch - 65) <= 5U)
                return num3 + 10;
            return -1;
        }

        internal char ScanControl()
        {
            if (CharsRight() <= 0)
                throw MakeException(Strings.GetString("MissingControl"));
            char ch1 = MoveRightGetChar();
            if (ch1 >= 97 && ch1 <= 122)
                ch1 -= ' ';
            char ch2;
            if ((ch2 = (char)((uint)ch1 - 64U)) < 32)
                return ch2;
            throw MakeException(Strings.GetString("UnrecognizedControl"));
        }

        internal bool IsOnlyTopOption(RegexOptions option)
        {
            if (option != RegexOptions.RightToLeft && option != RegexOptions.Compiled && option != RegexOptions.CultureInvariant)
                return option == RegexOptions.ECMAScript;
            return true;
        }

        internal void ScanOptions()
        {
            bool flag = false;
            while (CharsRight() > 0)
            {
                char ch = RightChar();
                switch (ch)
                {
                    case '-':
                        flag = true;
                        break;
                    case '+':
                        flag = false;
                        break;
                    default:
                        RegexOptions option = RegexParser.OptionFromCode(ch);
                        if (option == RegexOptions.None || IsOnlyTopOption(option))
                            return;
                        if (flag)
                        {
                            _options &= ~option;
                            break;
                        }
                        _options |= option;
                        break;
                }
                MoveRight();
            }
        }

        internal char ScanCharEscape()
        {
            char ch = MoveRightGetChar();
            if (ch >= 48 && ch <= 55)
            {
                MoveLeft();
                return ScanOctal();
            }
            switch (ch)
            {
                case 'a':
                    return '\a';
                case 'b':
                    return '\b';
                case 'c':
                    return ScanControl();
                case 'e':
                    return '\x001B';
                case 'f':
                    return '\f';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 't':
                    return '\t';
                case 'u':
                    return ScanHex(4);
                case 'v':
                    return '\v';
                case 'x':
                    return ScanHex(2);
                default:
                    if (!UseOptionE() && RegexCharClass.IsWordChar(ch))
                        throw MakeException(Strings.GetString("UnrecognizedEscape", (object)ch.ToString()));
                    return ch;
            }
        }

        internal string ParseProperty()
        {
            if (CharsRight() < 3)
                throw MakeException(Strings.GetString("IncompleteSlashP"));
            if (MoveRightGetChar() != 123)
                throw MakeException(Strings.GetString("MalformedSlashP"));
            int startIndex = Textpos();
            while (CharsRight() > 0)
            {
                char ch = MoveRightGetChar();
                if (!RegexCharClass.IsWordChar(ch) && ch != 45)
                {
                    MoveLeft();
                    break;
                }
            }
            string str = _pattern.Substring(startIndex, Textpos() - startIndex);
            if (CharsRight() == 0 || MoveRightGetChar() != 125)
                throw MakeException(Strings.GetString("IncompleteSlashP"));
            return str;
        }

        internal int TypeFromCode(char ch)
        {
            switch (ch)
            {
                case 'Z':
                    return 20;
                case 'b':
                    return !UseOptionE() ? 16 : 41;
                case 'z':
                    return 21;
                case 'A':
                    return 18;
                case 'B':
                    return !UseOptionE() ? 17 : 42;
                case 'G':
                    return 19;
                default:
                    return 22;
            }
        }

        internal static RegexOptions OptionFromCode(char ch)
        {
            if (ch >= 65 && ch <= 90)
                ch += ' ';
            switch (ch)
            {
                case 'm':
                    return RegexOptions.Multiline;
                case 'n':
                    return RegexOptions.ExplicitCapture;
                case 'r':
                    return RegexOptions.RightToLeft;
                case 's':
                    return RegexOptions.Singleline;
                case 'x':
                    return RegexOptions.IgnorePatternWhitespace;
                case 'c':
                    return RegexOptions.Compiled;
                case 'e':
                    return RegexOptions.ECMAScript;
                case 'i':
                    return RegexOptions.IgnoreCase;
                default:
                    return RegexOptions.None;
            }
        }

        internal void CountCaptures()
        {
            NoteCaptureSlot(0, 0);
            _autocap = 1;
            while (CharsRight() > 0)
            {
                int pos = Textpos();
                switch (MoveRightGetChar())
                {
                    case '#':
                        if (UseOptionX())
                        {
                            MoveLeft();
                            ScanBlank();
                            continue;
                        }
                        continue;
                    case '(':
                        if (CharsRight() >= 2 && RightChar(1) == 35 && RightChar() == 63)
                        {
                            MoveLeft();
                            ScanBlank();
                        }
                        else
                        {
                            PushOptions();
                            if (CharsRight() > 0 && RightChar() == 63)
                            {
                                MoveRight();
                                if (CharsRight() > 1 && (RightChar() == 60 || RightChar() == 39))
                                {
                                    MoveRight();
                                    char ch = RightChar();
                                    if (ch != 48 && RegexCharClass.IsWordChar(ch))
                                    {
                                        if (ch >= 49 && ch <= 57)
                                            NoteCaptureSlot(ScanDecimal(), pos);
                                        else
                                            NoteCaptureName(ScanCapname(), pos);
                                    }
                                }
                                else
                                {
                                    ScanOptions();
                                    if (CharsRight() > 0)
                                    {
                                        if (RightChar() == 41)
                                        {
                                            MoveRight();
                                            PopKeepOptions();
                                        }
                                        else if (RightChar() == 40)
                                        {
                                            _ignoreNextParen = true;
                                            continue;
                                        }
                                    }
                                }
                            }
                            else if (!UseOptionN() && !_ignoreNextParen)
                                NoteCaptureSlot(_autocap++, pos);
                        }
                        _ignoreNextParen = false;
                        continue;
                    case ')':
                        if (!EmptyOptionsStack())
                        {
                            PopOptions();
                            continue;
                        }
                        continue;
                    case '[':
                        ScanCharClass(false, true);
                        continue;
                    case '\\':
                        if (CharsRight() > 0)
                        {
                            MoveRight();
                            continue;
                        }
                        continue;
                    default:
                        continue;
                }
            }
            AssignNameSlots();
        }

        internal void NoteCaptureSlot(int i, int pos)
        {
            if (_caps.ContainsKey(i))
                return;
            _caps.Add(i, pos);
            ++_capcount;
            if (_captop > i)
                return;
            if (i == int.MaxValue)
                _captop = i;
            else
                _captop = i + 1;
        }

        internal void NoteCaptureName(string name, int pos)
        {
            if (_capnames == null)
            {
                _capnames = new Hashtable();
                _capnamelist = new List<string>();
            }
            if (_capnames.ContainsKey(name))
                return;
            _capnames.Add(name, pos);
            _capnamelist.Add(name);
        }

        internal void NoteCaptures(Hashtable caps, int capsize, Hashtable capnames)
        {
            _caps = caps;
            _capsize = capsize;
            _capnames = capnames;
        }

        internal void AssignNameSlots()
        {
            if (_capnames != null)
            {
                for (int index = 0; index < _capnamelist.Count; ++index)
                {
                    while (IsCaptureSlot(_autocap))
                        ++_autocap;
                    string str = _capnamelist[index];
                    var capname = (int)_capnames[str];
                    _capnames[str] = _autocap;
                    NoteCaptureSlot(_autocap, capname);
                    ++_autocap;
                }
            }
            if (_capcount < _captop)
            {
                _capnumlist = new int[_capcount];
                int num = 0;
                IDictionaryEnumerator enumerator = _caps.GetEnumerator();
                while (enumerator.MoveNext())
                    _capnumlist[num++] = (int)enumerator.Key;
                Array.Sort<int>(_capnumlist, Comparer<int>.Default);
            }
            if (_capnames == null && _capnumlist == null)
                return;
            int index1 = 0;
            List<string> stringList;
            int num1;
            if (_capnames == null)
            {
                stringList = null;
                _capnames = new Hashtable();
                _capnamelist = new List<string>();
                num1 = -1;
            }
            else
            {
                stringList = _capnamelist;
                _capnamelist = new List<string>();
                num1 = (int)_capnames[stringList[0]];
            }
            for (int index2 = 0; index2 < _capcount; ++index2)
            {
                int num2 = _capnumlist == null ? index2 : _capnumlist[index2];
                if (num1 == num2)
                {
                    _capnamelist.Add(stringList[index1++]);
                    num1 = index1 == stringList.Count ? -1 : (int)_capnames[stringList[index1]];
                }
                else
                {
                    string str = Convert.ToString(num2, _culture);
                    _capnamelist.Add(str);
                    _capnames[str] = num2;
                }
            }
        }

        internal int CaptureSlotFromName(string capname)
        {
            return (int)_capnames[capname];
        }

        internal bool IsCaptureSlot(int i)
        {
            if (_caps != null)
                return _caps.ContainsKey(i);
            if (i >= 0)
                return i < _capsize;
            return false;
        }

        internal bool IsCaptureName(string capname)
        {
            if (_capnames == null)
                return false;
            return _capnames.ContainsKey(capname);
        }

        internal bool UseOptionN()
        {
            return (_options & RegexOptions.ExplicitCapture) != RegexOptions.None;
        }

        internal bool UseOptionI()
        {
            return (_options & RegexOptions.IgnoreCase) != RegexOptions.None;
        }

        internal bool UseOptionM()
        {
            return (_options & RegexOptions.Multiline) != RegexOptions.None;
        }

        internal bool UseOptionS()
        {
            return (_options & RegexOptions.Singleline) != RegexOptions.None;
        }

        internal bool UseOptionX()
        {
            return (_options & RegexOptions.IgnorePatternWhitespace) != RegexOptions.None;
        }

        internal bool UseOptionE()
        {
            return (_options & RegexOptions.ECMAScript) != RegexOptions.None;
        }

        internal static bool IsSpecial(char ch)
        {
            if (ch <= 124)
                return RegexParser._category[(int)ch] >= 4;
            return false;
        }

        internal static bool IsStopperX(char ch)
        {
            if (ch <= 124)
                return RegexParser._category[(int)ch] >= 2;
            return false;
        }

        internal static bool IsQuantifier(char ch)
        {
            if (ch <= 123)
                return RegexParser._category[(int)ch] >= 5;
            return false;
        }

        internal bool IsTrueQuantifier()
        {
            int num1 = CharsRight();
            if (num1 == 0)
                return false;
            int i = Textpos();
            char ch = CharAt(i);
            if (ch != 123)
            {
                if (ch <= 123)
                    return RegexParser._category[(int)ch] >= 5;
                return false;
            }
            int num2 = i;
            while (--num1 > 0 && (int)(ch = CharAt(++num2)) >= 48 && (int)ch <= 57);
            if (num1 == 0 || num2 - i == 1)
                return false;
            if (ch == 125)
                return true;
            if (ch != 44)
                return false;
            while (--num1 > 0 && (int)(ch = CharAt(++num2)) >= 48 && (int)ch <= 57);
            if (num1 > 0)
                return ch == 125;
            return false;
        }

        internal static bool IsSpace(char ch)
        {
            if (ch <= 32)
                return RegexParser._category[(int)ch] == 2;
            return false;
        }

        internal static bool IsMetachar(char ch)
        {
            if (ch <= 124)
                return RegexParser._category[(int)ch] >= 1;
            return false;
        }

        internal void AddConcatenate(int pos, int cch, bool isReplacement)
        {
            if (cch == 0)
                return;
            RegexNode newChild;
            if (cch > 1)
            {
                string str = _pattern.Substring(pos, cch);
                if (UseOptionI() && !isReplacement)
                {
                    var stringBuilder = new StringBuilder(str.Length);
                    for (int index = 0; index < str.Length; ++index)
                        stringBuilder.Append(char.ToLower(str[index], _culture));
                    str = stringBuilder.ToString();
                }
                newChild = new RegexNode(12, _options, str);
            }
            else
            {
                char lower = _pattern[pos];
                if (UseOptionI() && !isReplacement)
                    lower = char.ToLower(lower, _culture);
                newChild = new RegexNode(9, _options, lower);
            }
            _concatenation.AddChild(newChild);
        }

        internal void PushGroup()
        {
            _group._next = _stack;
            _alternation._next = _group;
            _concatenation._next = _alternation;
            _stack = _concatenation;
        }

        internal void PopGroup()
        {
            _concatenation = _stack;
            _alternation = _concatenation._next;
            _group = _alternation._next;
            _stack = _group._next;
            if (_group.Type() != 34 || _group.ChildCount() != 0)
                return;
            if (_unit == null)
                throw MakeException(Strings.GetString("IllegalCondition"));
            _group.AddChild(_unit);
            _unit = null;
        }

        internal bool EmptyStack()
        {
            return _stack == null;
        }

        internal void StartGroup(RegexNode openGroup)
        {
            _group = openGroup;
            _alternation = new RegexNode(24, _options);
            _concatenation = new RegexNode(25, _options);
        }

        internal void AddAlternate()
        {
            if (_group.Type() == 34 || _group.Type() == 33)
                _group.AddChild(_concatenation.ReverseLeft());
            else
                _alternation.AddChild(_concatenation.ReverseLeft());
            _concatenation = new RegexNode(25, _options);
        }

        internal void AddConcatenate()
        {
            _concatenation.AddChild(_unit);
            _unit = null;
        }

        internal void AddConcatenate(bool lazy, int min, int max)
        {
            _concatenation.AddChild(_unit.MakeQuantifier(lazy, min, max));
            _unit = null;
        }

        internal RegexNode Unit()
        {
            return _unit;
        }

        internal void AddUnitOne(char ch)
        {
            if (UseOptionI())
                ch = char.ToLower(ch, _culture);
            _unit = new RegexNode(9, _options, ch);
        }

        internal void AddUnitNotone(char ch)
        {
            if (UseOptionI())
                ch = char.ToLower(ch, _culture);
            _unit = new RegexNode(10, _options, ch);
        }

        internal void AddUnitSet(string cc)
        {
            _unit = new RegexNode(11, _options, cc);
        }

        internal void AddUnitNode(RegexNode node)
        {
            _unit = node;
        }

        internal void AddUnitType(int type)
        {
            _unit = new RegexNode(type, _options);
        }

        internal void AddGroup()
        {
            if (_group.Type() == 34 || _group.Type() == 33)
            {
                _group.AddChild(_concatenation.ReverseLeft());
                if (_group.Type() == 33 && _group.ChildCount() > 2 || _group.ChildCount() > 3)
                    throw MakeException(Strings.GetString("TooManyAlternates"));
            }
            else
            {
                _alternation.AddChild(_concatenation.ReverseLeft());
                _group.AddChild(_alternation);
            }
            _unit = _group;
        }

        internal void PushOptions()
        {
            _optionsStack.Add(_options);
        }

        internal void PopOptions()
        {
            _options = _optionsStack[_optionsStack.Count - 1];
            _optionsStack.RemoveAt(_optionsStack.Count - 1);
        }

        internal bool EmptyOptionsStack()
        {
            return _optionsStack.Count == 0;
        }

        internal void PopKeepOptions()
        {
            _optionsStack.RemoveAt(_optionsStack.Count - 1);
        }

        internal ArgumentException MakeException(string message)
        {
            return new ArgumentException(Strings.GetString(nameof(MakeException), _pattern, message));
        }

        internal int Textpos()
        {
            return _currentPos;
        }

        internal void Textto(int pos)
        {
            _currentPos = pos;
        }

        internal char MoveRightGetChar()
        {
            return _pattern[_currentPos++];
        }

        internal void MoveRight()
        {
            MoveRight(1);
        }

        internal void MoveRight(int i)
        {
            _currentPos += i;
        }

        internal void MoveLeft()
        {
            --_currentPos;
        }

        internal char CharAt(int i)
        {
            return _pattern[i];
        }

        internal char RightChar()
        {
            return _pattern[_currentPos];
        }

        internal char RightChar(int i)
        {
            return _pattern[_currentPos + i];
        }

        internal int CharsRight()
        {
            return _pattern.Length - _currentPos;
        }
    }
}
