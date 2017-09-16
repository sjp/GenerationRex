using System.Collections.Generic;
using System.Globalization;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexNode
    {
        internal const int Oneloop = 3;
        internal const int Notoneloop = 4;
        internal const int Setloop = 5;
        internal const int Onelazy = 6;
        internal const int Notonelazy = 7;
        internal const int Setlazy = 8;
        internal const int One = 9;
        internal const int Notone = 10;
        internal const int Set = 11;
        internal const int Multi = 12;
        internal const int Ref = 13;
        internal const int Bol = 14;
        internal const int Eol = 15;
        internal const int Boundary = 16;
        internal const int Nonboundary = 17;
        internal const int ECMABoundary = 41;
        internal const int NonECMABoundary = 42;
        internal const int Beginning = 18;
        internal const int Start = 19;
        internal const int EndZ = 20;
        internal const int End = 21;
        internal const int Nothing = 22;
        internal const int Empty = 23;
        internal const int Alternate = 24;
        internal const int Concatenate = 25;
        internal const int Loop = 26;
        internal const int Lazyloop = 27;
        internal const int Capture = 28;
        internal const int Group = 29;
        internal const int Require = 30;
        internal const int Prevent = 31;
        internal const int Greedy = 32;
        internal const int Testref = 33;
        internal const int Testgroup = 34;
        internal int _type;
        internal List<RegexNode> _children;
        internal string _str;
        internal char _ch;
        internal int _m;
        internal int _n;
        internal RegexOptions _options;
        internal RegexNode _next;

        internal RegexNode(int type, RegexOptions options)
        {
            _type = type;
            _options = options;
        }

        internal RegexNode(int type, RegexOptions options, char ch)
        {
            _type = type;
            _options = options;
            _ch = ch;
        }

        internal RegexNode(int type, RegexOptions options, string str)
        {
            _type = type;
            _options = options;
            _str = str;
        }

        internal RegexNode(int type, RegexOptions options, int m)
        {
            _type = type;
            _options = options;
            _m = m;
        }

        internal RegexNode(int type, RegexOptions options, int m, int n)
        {
            _type = type;
            _options = options;
            _m = m;
            _n = n;
        }

        internal bool UseOptionR()
        {
            return (_options & RegexOptions.RightToLeft) != RegexOptions.None;
        }

        internal RegexNode ReverseLeft()
        {
            if (UseOptionR() && _type == 25 && _children != null)
                _children.Reverse(0, _children.Count);
            return this;
        }

        internal void MakeRep(int type, int min, int max)
        {
            _type += type - 9;
            _m = min;
            _n = max;
        }

        internal RegexNode Reduce()
        {
            RegexNode regexNode;
            switch (Type())
            {
                case 5:
                case 11:
                    regexNode = ReduceSet();
                    break;
                case 24:
                    regexNode = ReduceAlternation();
                    break;
                case 25:
                    regexNode = ReduceConcatenation();
                    break;
                case 26:
                case 27:
                    regexNode = ReduceRep();
                    break;
                case 29:
                    regexNode = ReduceGroup();
                    break;
                default:
                    regexNode = this;
                    break;
            }
            return regexNode;
        }

        internal RegexNode StripEnation(int emptyType)
        {
            switch (ChildCount())
            {
                case 0:
                    return new RegexNode(emptyType, _options);
                case 1:
                    return Child(0);
                default:
                    return this;
            }
        }

        internal RegexNode ReduceGroup()
        {
            var regexNode = this;
            while (regexNode.Type() == 29)
                regexNode = regexNode.Child(0);
            return regexNode;
        }

        internal RegexNode ReduceRep()
        {
            var regexNode1 = this;
            int num1 = Type();
            int num2 = _m;
            int num3 = _n;
            while (regexNode1.ChildCount() != 0)
            {
                RegexNode regexNode2 = regexNode1.Child(0);
                if (regexNode2.Type() != num1)
                {
                    int num4 = regexNode2.Type();
                    if ((num4 < 3 || num4 > 5 || num1 != 26) && (num4 < 6 || num4 > 8 || num1 != 27))
                        break;
                }
                if ((regexNode1._m != 0 || regexNode2._m <= 1) && regexNode2._n >= regexNode2._m * 2)
                {
                    regexNode1 = regexNode2;
                    if (regexNode1._m > 0)
                        regexNode1._m = num2 = 2147483646 / regexNode1._m < num2 ? int.MaxValue : regexNode1._m * num2;
                    if (regexNode1._n > 0)
                        regexNode1._n = num3 = 2147483646 / regexNode1._n < num3 ? int.MaxValue : regexNode1._n * num3;
                }
                else
                    break;
            }
            if (num2 != int.MaxValue)
                return regexNode1;
            return new RegexNode(22, _options);
        }

        internal RegexNode ReduceSet()
        {
            if (RegexCharClass.IsEmpty(_str))
            {
                _type = 22;
                _str = null;
            }
            else if (RegexCharClass.IsSingleton(_str))
            {
                _ch = RegexCharClass.SingletonChar(_str);
                _str = null;
                _type += -2;
            }
            else if (RegexCharClass.IsSingletonInverse(_str))
            {
                _ch = RegexCharClass.SingletonChar(_str);
                _str = null;
                _type += -1;
            }
            return this;
        }

        internal RegexNode ReduceAlternation()
        {
            if (_children == null)
                return new RegexNode(22, _options);
            bool flag1 = false;
            bool flag2 = false;
            var regexOptions1 = RegexOptions.None;
            int index1 = 0;
            int index2 = 0;
            while (index1 < _children.Count)
            {
                RegexNode child1 = _children[index1];
                if (index2 < index1)
                    _children[index2] = child1;
                if (child1._type == 24)
                {
                    for (int index3 = 0; index3 < child1._children.Count; ++index3)
                        child1._children[index3]._next = this;
                    _children.InsertRange(index1 + 1, child1._children);
                    --index2;
                }
                else if (child1._type == 11 || child1._type == 9)
                {
                    RegexOptions regexOptions2 = child1._options & (RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
                    if (child1._type == 11)
                    {
                        if (!flag1 || regexOptions1 != regexOptions2 || (flag2 || !RegexCharClass.IsMergeable(child1._str)))
                        {
                            flag1 = true;
                            flag2 = !RegexCharClass.IsMergeable(child1._str);
                            regexOptions1 = regexOptions2;
                            goto label_26;
                        }
                    }
                    else if (!flag1 || regexOptions1 != regexOptions2 || flag2)
                    {
                        flag1 = true;
                        flag2 = false;
                        regexOptions1 = regexOptions2;
                        goto label_26;
                    }
                    --index2;
                    RegexNode child2 = _children[index2];
                    RegexCharClass regexCharClass;
                    if (child2._type == 9)
                    {
                        regexCharClass = new RegexCharClass();
                        regexCharClass.AddChar(child2._ch);
                    }
                    else
                        regexCharClass = RegexCharClass.Parse(child2._str);
                    if (child1._type == 9)
                    {
                        regexCharClass.AddChar(child1._ch);
                    }
                    else
                    {
                        RegexCharClass cc = RegexCharClass.Parse(child1._str);
                        regexCharClass.AddCharClass(cc);
                    }
                    child2._type = 11;
                    child2._str = regexCharClass.ToStringClass();
                }
                else if (child1._type == 22)
                {
                    --index2;
                }
                else
                {
                    flag1 = false;
                    flag2 = false;
                }
                label_26:
                ++index1;
                ++index2;
            }
            if (index2 < index1)
                _children.RemoveRange(index2, index1 - index2);
            return StripEnation(22);
        }

        internal RegexNode ReduceConcatenation()
        {
            if (_children == null)
                return new RegexNode(23, _options);
            bool flag = false;
            var regexOptions1 = RegexOptions.None;
            int index1 = 0;
            int index2 = 0;
            while (index1 < _children.Count)
            {
                RegexNode child1 = _children[index1];
                if (index2 < index1)
                    _children[index2] = child1;
                if (child1._type == 25 && (child1._options & RegexOptions.RightToLeft) == (_options & RegexOptions.RightToLeft))
                {
                    for (int index3 = 0; index3 < child1._children.Count; ++index3)
                        child1._children[index3]._next = this;
                    _children.InsertRange(index1 + 1, child1._children);
                    --index2;
                }
                else if (child1._type == 12 || child1._type == 9)
                {
                    RegexOptions regexOptions2 = child1._options & (RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
                    if (!flag || regexOptions1 != regexOptions2)
                    {
                        flag = true;
                        regexOptions1 = regexOptions2;
                    }
                    else
                    {
                        RegexNode child2 = _children[--index2];
                        if (child2._type == 9)
                        {
                            child2._type = 12;
                            child2._str = Convert.ToString(child2._ch, CultureInfo.InvariantCulture);
                        }
                        if ((regexOptions2 & RegexOptions.RightToLeft) == RegexOptions.None)
                        {
                            if (child1._type == 9)
                                child2._str += child1._ch.ToString();
                            else
                                child2._str += child1._str;
                        }
                        else
                            child2._str = child1._type != 9 ? child1._str + child2._str : child1._ch.ToString() + child2._str;
                    }
                }
                else if (child1._type == 23)
                    --index2;
                else
                    flag = false;
                ++index1;
                ++index2;
            }
            if (index2 < index1)
                _children.RemoveRange(index2, index1 - index2);
            return StripEnation(23);
        }

        internal RegexNode MakeQuantifier(bool lazy, int min, int max)
        {
            if (min == 0 && max == 0)
                return new RegexNode(23, _options);
            if (min == 1 && max == 1)
                return this;
            switch (_type)
            {
                case 9:
                case 10:
                case 11:
                    MakeRep(lazy ? 6 : 3, min, max);
                    return this;
                default:
                    var regexNode = new RegexNode(lazy ? 27 : 26, _options, min, max);
                    regexNode.AddChild(this);
                    return regexNode;
            }
        }

        internal void AddChild(RegexNode newChild)
        {
            if (_children == null)
                _children = new List<RegexNode>(4);
            RegexNode regexNode = newChild.Reduce();
            _children.Add(regexNode);
            regexNode._next = this;
        }

        internal RegexNode Child(int i)
        {
            return _children[i];
        }

        internal int ChildCount()
        {
            if (_children != null)
                return _children.Count;
            return 0;
        }

        internal int Type()
        {
            return _type;
        }
    }
}
