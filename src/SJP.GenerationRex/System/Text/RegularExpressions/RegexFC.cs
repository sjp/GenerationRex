using System.Globalization;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexFC
    {
        internal RegexCharClass _cc;
        internal bool _nullable;
        internal bool _caseInsensitive;

        internal RegexFC(bool nullable)
        {
            _cc = new RegexCharClass();
            _nullable = nullable;
        }

        internal RegexFC(char ch, bool not, bool nullable, bool caseInsensitive)
        {
            _cc = new RegexCharClass();
            if (not)
            {
                if (ch > 0)
                    _cc.AddRange(char.MinValue, (char)(ch - 1U));
                if (ch < ushort.MaxValue)
                    _cc.AddRange((char)(ch + 1U), char.MaxValue);
            }
            else
                _cc.AddRange(ch, ch);
            _caseInsensitive = caseInsensitive;
            _nullable = nullable;
        }

        internal RegexFC(string charClass, bool nullable, bool caseInsensitive)
        {
            _cc = RegexCharClass.Parse(charClass);
            _nullable = nullable;
            _caseInsensitive = caseInsensitive;
        }

        internal bool AddFC(RegexFC fc, bool concatenate)
        {
            if (!_cc.CanMerge || !fc._cc.CanMerge)
                return false;
            if (concatenate)
            {
                if (!_nullable)
                    return true;
                if (!fc._nullable)
                    _nullable = false;
            }
            else if (fc._nullable)
                _nullable = true;
            _caseInsensitive |= fc._caseInsensitive;
            _cc.AddCharClass(fc._cc);
            return true;
        }

        internal string GetFirstChars(CultureInfo culture)
        {
            if (_caseInsensitive)
                _cc.AddLowercase(culture);
            return _cc.ToStringClass();
        }

        internal bool IsCaseInsensitive()
        {
            return _caseInsensitive;
        }
    }
}
