using System.Globalization;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexFCD
    {
        private const int BeforeChild = 64;
        private const int AfterChild = 128;
        internal const int Beginning = 1;
        internal const int Bol = 2;
        internal const int Start = 4;
        internal const int Eol = 8;
        internal const int EndZ = 16;
        internal const int End = 32;
        internal const int Boundary = 64;
        internal const int ECMABoundary = 128;
        private int[] _intStack;
        private int _intDepth;
        private RegexFC[] _fcStack;
        private int _fcDepth;
        private bool _skipAllChildren;
        private bool _skipchild;
        private bool _failed;

        internal static RegexPrefix FirstChars(RegexTree t)
        {
            RegexFC regexFc = new RegexFCD().RegexFCFromRegexTree(t);
            if (regexFc == null || regexFc._nullable)
                return null;
            CultureInfo culture = (t._options & RegexOptions.CultureInvariant) != RegexOptions.None ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
            return new RegexPrefix(regexFc.GetFirstChars(culture), regexFc.IsCaseInsensitive());
        }

        internal static RegexPrefix Prefix(RegexTree tree)
        {
            RegexNode regexNode1 = null;
            int num = 0;
            RegexNode regexNode2 = tree._root;
            while (true)
            {
                switch (regexNode2._type)
                {
                    case 3:
                    case 6:
                        goto label_5;
                    case 9:
                        goto label_8;
                    case 12:
                        goto label_9;
                    case 14:
                    case 15:
                    case 16:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 23:
                    case 30:
                    case 31:
                    case 41:
                        if (regexNode1 != null && num < regexNode1.ChildCount())
                        {
                            regexNode2 = regexNode1.Child(num++);
                            continue;
                        }
                        goto label_12;
                    case 25:
                        if (regexNode2.ChildCount() > 0)
                        {
                            regexNode1 = regexNode2;
                            num = 0;
                            goto case 14;
                        }
                        else
                            goto case 14;
                    case 28:
                    case 32:
                        regexNode2 = regexNode2.Child(0);
                        regexNode1 = null;
                        continue;
                    default:
                        goto label_10;
                }
            }
            label_5:
            if (regexNode2._m > 0)
                return new RegexPrefix(string.Empty.PadRight(regexNode2._m, regexNode2._ch), RegexOptions.None != (regexNode2._options & RegexOptions.IgnoreCase));
            return RegexPrefix.Empty;
            label_8:
            return new RegexPrefix(regexNode2._ch.ToString(CultureInfo.InvariantCulture), RegexOptions.None != (regexNode2._options & RegexOptions.IgnoreCase));
            label_9:
            return new RegexPrefix(regexNode2._str, RegexOptions.None != (regexNode2._options & RegexOptions.IgnoreCase));
            label_10:
            return RegexPrefix.Empty;
            label_12:
            return RegexPrefix.Empty;
        }

        internal static int Anchors(RegexTree tree)
        {
            RegexNode regexNode1 = null;
            int num1 = 0;
            int num2 = 0;
            RegexNode regexNode2 = tree._root;
            while (true)
            {
                switch (regexNode2._type)
                {
                    case 14:
                    case 15:
                    case 16:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 41:
                        goto label_5;
                    case 23:
                    case 30:
                    case 31:
                        if (regexNode1 != null && num1 < regexNode1.ChildCount())
                        {
                            regexNode2 = regexNode1.Child(num1++);
                            continue;
                        }
                        goto label_8;
                    case 25:
                        if (regexNode2.ChildCount() > 0)
                        {
                            regexNode1 = regexNode2;
                            num1 = 0;
                            goto case 23;
                        }
                        else
                            goto case 23;
                    case 28:
                    case 32:
                        regexNode2 = regexNode2.Child(0);
                        regexNode1 = null;
                        continue;
                    default:
                        goto label_6;
                }
            }
            label_5:
            return num2 | RegexFCD.AnchorFromType(regexNode2._type);
            label_6:
            return num2;
            label_8:
            return num2;
        }

        private static int AnchorFromType(int type)
        {
            switch (type)
            {
                case 14:
                    return 2;
                case 15:
                    return 8;
                case 16:
                    return 64;
                case 18:
                    return 1;
                case 19:
                    return 4;
                case 20:
                    return 16;
                case 21:
                    return 32;
                case 41:
                    return 128;
                default:
                    return 0;
            }
        }

        private RegexFCD()
        {
            _fcStack = new RegexFC[32];
            _intStack = new int[32];
        }

        private void PushInt(int I)
        {
            if (_intDepth >= _intStack.Length)
            {
                var numArray = new int[_intDepth * 2];
                Array.Copy(_intStack, 0, numArray, 0, _intDepth);
                _intStack = numArray;
            }
            _intStack[_intDepth++] = I;
        }

        private bool IntIsEmpty()
        {
            return _intDepth == 0;
        }

        private int PopInt()
        {
            return _intStack[--_intDepth];
        }

        private void PushFC(RegexFC fc)
        {
            if (_fcDepth >= _fcStack.Length)
            {
                var regexFcArray = new RegexFC[_fcDepth * 2];
                Array.Copy(_fcStack, 0, regexFcArray, 0, _fcDepth);
                _fcStack = regexFcArray;
            }
            _fcStack[_fcDepth++] = fc;
        }

        private bool FCIsEmpty()
        {
            return _fcDepth == 0;
        }

        private RegexFC PopFC()
        {
            return _fcStack[--_fcDepth];
        }

        private RegexFC TopFC()
        {
            return _fcStack[_fcDepth - 1];
        }

        private RegexFC RegexFCFromRegexTree(RegexTree tree)
        {
            RegexNode node = tree._root;
            int index = 0;
            while (true)
            {
                while (node._children != null)
                {
                    if (index < node._children.Count && !_skipAllChildren)
                    {
                        CalculateFC(node._type | 64, node, index);
                        if (!_skipchild)
                        {
                            node = node._children[index];
                            PushInt(index);
                            index = 0;
                        }
                        else
                        {
                            ++index;
                            _skipchild = false;
                        }
                    }
                    else
                        goto label_7;
                }
                CalculateFC(node._type, node, 0);
                label_7:
                _skipAllChildren = false;
                if (!IntIsEmpty())
                {
                    int CurIndex = PopInt();
                    node = node._next;
                    CalculateFC(node._type | 128, node, CurIndex);
                    if (!_failed)
                        index = CurIndex + 1;
                    else
                        break;
                }
                else
                    goto label_11;
            }
            return null;
            label_11:
            if (FCIsEmpty())
                return null;
            return PopFC();
        }

        private void SkipChild()
        {
            _skipchild = true;
        }

        private void CalculateFC(int NodeType, RegexNode node, int CurIndex)
        {
            bool caseInsensitive = false;
            bool flag = false;
            if (NodeType <= 13)
            {
                if ((node._options & RegexOptions.IgnoreCase) != RegexOptions.None)
                    caseInsensitive = true;
                if ((node._options & RegexOptions.RightToLeft) != RegexOptions.None)
                    flag = true;
            }
            switch (NodeType)
            {
                case 3:
                case 6:
                    PushFC(new RegexFC(node._ch, false, node._m == 0, caseInsensitive));
                    break;
                case 4:
                case 7:
                    PushFC(new RegexFC(node._ch, true, node._m == 0, caseInsensitive));
                    break;
                case 5:
                case 8:
                    PushFC(new RegexFC(node._str, node._m == 0, caseInsensitive));
                    break;
                case 9:
                case 10:
                    PushFC(new RegexFC(node._ch, NodeType == 10, false, caseInsensitive));
                    break;
                case 11:
                    PushFC(new RegexFC(node._str, false, caseInsensitive));
                    break;
                case 12:
                    if (node._str.Length == 0)
                    {
                        PushFC(new RegexFC(true));
                        break;
                    }
                    if (!flag)
                    {
                        PushFC(new RegexFC(node._str[0], false, false, caseInsensitive));
                        break;
                    }
                    PushFC(new RegexFC(node._str[node._str.Length - 1], false, false, caseInsensitive));
                    break;
                case 13:
                    PushFC(new RegexFC("\0\x0001\0\0", true, false));
                    break;
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 41:
                case 42:
                    PushFC(new RegexFC(true));
                    break;
                case 23:
                    PushFC(new RegexFC(true));
                    break;
                case 88:
                    break;
                case 89:
                    break;
                case 90:
                    break;
                case 91:
                    break;
                case 92:
                    break;
                case 93:
                    break;
                case 94:
                case 95:
                    SkipChild();
                    PushFC(new RegexFC(true));
                    break;
                case 96:
                    break;
                case 97:
                    break;
                case 98:
                    if (CurIndex != 0)
                        break;
                    SkipChild();
                    break;
                case 152:
                case 161:
                    if (CurIndex == 0)
                        break;
                    _failed = !TopFC().AddFC(PopFC(), false);
                    break;
                case 153:
                    if (CurIndex != 0)
                        _failed = !TopFC().AddFC(PopFC(), true);
                    if (TopFC()._nullable)
                        break;
                    _skipAllChildren = true;
                    break;
                case 154:
                case 155:
                    if (node._m != 0)
                        break;
                    TopFC()._nullable = true;
                    break;
                case 156:
                    break;
                case 157:
                    break;
                case 158:
                    break;
                case 159:
                    break;
                case 160:
                    break;
                case 162:
                    if (CurIndex <= 1)
                        break;
                    _failed = !TopFC().AddFC(PopFC(), false);
                    break;
                default:
                    throw new ArgumentException(Strings.GetString("UnexpectedOpcode", (object)NodeType.ToString(CultureInfo.CurrentCulture)));
            }
        }
    }
}
