using System.Collections;
using System.Collections.Generic;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexReplacement
    {
        internal const int Specials = 4;
        internal const int LeftPortion = -1;
        internal const int RightPortion = -2;
        internal const int LastGroup = -3;
        internal const int WholeString = -4;
        internal string _rep;
        internal List<string> _strings;
        internal List<int> _rules;

        internal RegexReplacement(string rep, RegexNode concat, Hashtable _caps)
        {
            _rep = rep;
            if (concat.Type() != 25)
                throw new ArgumentException(Strings.GetString("ReplacementError"));
            var stringBuilder = new StringBuilder();
            var stringList = new List<string>();
            var intList = new List<int>();
            for (int i = 0; i < concat.ChildCount(); ++i)
            {
                RegexNode regexNode = concat.Child(i);
                switch (regexNode.Type())
                {
                    case 9:
                        stringBuilder.Append(regexNode._ch);
                        break;
                    case 12:
                        stringBuilder.Append(regexNode._str);
                        break;
                    case 13:
                        if (stringBuilder.Length > 0)
                        {
                            intList.Add(stringList.Count);
                            stringList.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                        }
                        int num = regexNode._m;
                        if (_caps != null && num >= 0)
                            num = (int)_caps[num];
                        intList.Add(-5 - num);
                        break;
                    default:
                        throw new ArgumentException(Strings.GetString("ReplacementError"));
                }
            }
            if (stringBuilder.Length > 0)
            {
                intList.Add(stringList.Count);
                stringList.Add(stringBuilder.ToString());
            }
            _strings = stringList;
            _rules = intList;
        }

        internal string Pattern
        {
            get
            {
                return _rep;
            }
        }
    }
}
