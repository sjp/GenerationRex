using System.Globalization;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexBoyerMoore
    {
        internal const int infinite = 2147483647;
        internal int[] _positive;
        internal int[] _negativeASCII;
        internal int[][] _negativeUnicode;
        internal string _pattern;
        internal int _lowASCII;
        internal int _highASCII;
        internal bool _rightToLeft;
        internal bool _caseInsensitive;
        internal CultureInfo _culture;

        internal RegexBoyerMoore(string pattern, bool caseInsensitive, bool rightToLeft, CultureInfo culture)
        {
            if (caseInsensitive)
            {
                var stringBuilder = new StringBuilder(pattern.Length);
                for (int index = 0; index < pattern.Length; ++index)
                    stringBuilder.Append(char.ToLower(pattern[index], culture));
                pattern = stringBuilder.ToString();
            }
            _pattern = pattern;
            _rightToLeft = rightToLeft;
            _caseInsensitive = caseInsensitive;
            _culture = culture;
            int num1;
            int num2;
            int num3;
            if (!rightToLeft)
            {
                num1 = -1;
                num2 = pattern.Length - 1;
                num3 = 1;
            }
            else
            {
                num1 = pattern.Length;
                num2 = 0;
                num3 = -1;
            }
            _positive = new int[pattern.Length];
            int index1 = num2;
            char ch1 = pattern[index1];
            _positive[index1] = num3;
            int index2 = index1 - num3;
            while (index2 != num1)
            {
                if (pattern[index2] != ch1)
                {
                    index2 -= num3;
                }
                else
                {
                    int index3 = num2;
                    int index4 = index2;
                    while (index4 != num1 && pattern[index3] == pattern[index4])
                    {
                        index4 -= num3;
                        index3 -= num3;
                    }
                    if (_positive[index3] == 0)
                        _positive[index3] = index3 - index4;
                    index2 -= num3;
                }
            }
            int index5 = num2 - num3;
            while (index5 != num1)
            {
                if (_positive[index5] == 0)
                    _positive[index5] = num3;
                index5 -= num3;
            }
            _negativeASCII = new int[128];
            for (int index3 = 0; index3 < 128; ++index3)
                _negativeASCII[index3] = num2 - num1;
            _lowASCII = sbyte.MaxValue;
            _highASCII = 0;
            int index6 = num2;
            while (index6 != num1)
            {
                char ch2 = pattern[index6];
                if (ch2 < 128)
                {
                    if (_lowASCII > ch2)
                        _lowASCII = ch2;
                    if (_highASCII < ch2)
                        _highASCII = ch2;
                    if (_negativeASCII[ch2] == num2 - num1)
                        _negativeASCII[ch2] = num2 - index6;
                }
                else
                {
                    int index3 = ch2 >> 8;
                    int index4 = ch2 & byte.MaxValue;
                    if (_negativeUnicode == null)
                        _negativeUnicode = new int[256][];
                    if (_negativeUnicode[index3] == null)
                    {
                        var numArray = new int[256];
                        for (int index7 = 0; index7 < 256; ++index7)
                            numArray[index7] = num2 - num1;
                        if (index3 == 0)
                        {
                            Array.Copy(_negativeASCII, numArray, 128);
                            _negativeASCII = numArray;
                        }
                        _negativeUnicode[index3] = numArray;
                    }
                    if (_negativeUnicode[index3][index4] == num2 - num1)
                        _negativeUnicode[index3][index4] = num2 - index6;
                }
                index6 -= num3;
            }
        }

        private bool MatchPattern(string text, int index)
        {
            if (!_caseInsensitive)
                return 0 == string.CompareOrdinal(_pattern, 0, text, index, _pattern.Length);
            if (text.Length - index < _pattern.Length)
                return false;
            TextInfo textInfo = _culture.TextInfo;
            for (int index1 = 0; index1 < _pattern.Length; ++index1)
            {
                if (textInfo.ToLower(text[index + index1]) != _pattern[index1])
                    return false;
            }
            return true;
        }

        internal bool IsMatch(string text, int index, int beglimit, int endlimit)
        {
            if (!_rightToLeft)
            {
                if (index < beglimit || endlimit - index < _pattern.Length)
                    return false;
                return MatchPattern(text, index);
            }
            if (index > endlimit || index - beglimit < _pattern.Length)
                return false;
            return MatchPattern(text, index - _pattern.Length);
        }

        internal int Scan(string text, int index, int beglimit, int endlimit)
        {
            int num1;
            int index1;
            int num2;
            int index2;
            int num3;
            if (!_rightToLeft)
            {
                num1 = _pattern.Length;
                index1 = _pattern.Length - 1;
                num2 = 0;
                index2 = index + num1 - 1;
                num3 = 1;
            }
            else
            {
                num1 = -_pattern.Length;
                index1 = 0;
                num2 = -num1 - 1;
                index2 = index + num1;
                num3 = -1;
            }
            char ch = _pattern[index1];
            label_4:
            while (index2 < endlimit && index2 >= beglimit)
            {
                char lower1 = text[index2];
                if (_caseInsensitive)
                    lower1 = char.ToLower(lower1, _culture);
                if (lower1 != ch)
                {
                    int[] numArray;
                    int num4 = lower1 >= 128 ? (_negativeUnicode == null || (numArray = _negativeUnicode[lower1 >> 8]) == null ? num1 : numArray[lower1 & byte.MaxValue]) : _negativeASCII[lower1];
                    index2 += num4;
                }
                else
                {
                    int index3 = index2;
                    int index4 = index1;
                    while (index4 != num2)
                    {
                        index4 -= num3;
                        index3 -= num3;
                        char lower2 = text[index3];
                        if (_caseInsensitive)
                            lower2 = char.ToLower(lower2, _culture);
                        if (lower2 != _pattern[index4])
                        {
                            int num4 = _positive[index4];
                            int num5;
                            if ((lower2 & 65408) == 0)
                            {
                                num5 = index4 - index1 + _negativeASCII[lower2];
                            }
                            else
                            {
                                int[] numArray;
                                if (_negativeUnicode != null && (numArray = _negativeUnicode[lower2 >> 8]) != null)
                                {
                                    num5 = index4 - index1 + numArray[lower2 & byte.MaxValue];
                                }
                                else
                                {
                                    index2 += num4;
                                    goto label_4;
                                }
                            }
                            if ((_rightToLeft ? (num5 < num4 ? 1 : 0) : (num5 > num4 ? 1 : 0)) != 0)
                                num4 = num5;
                            index2 += num4;
                            goto label_4;
                        }
                    }
                    if (!_rightToLeft)
                        return index3;
                    return index3 + 1;
                }
            }
            return -1;
        }

        public override string ToString()
        {
            return _pattern;
        }
    }
}
