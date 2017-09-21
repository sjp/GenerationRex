using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.GenerationRex
{
    internal class BddBuilder : ICharacterConstraintSolver<BinaryDecisionDiagram>
    {
        private readonly Dictionary<long, BinaryDecisionDiagram> _restrictCache = new Dictionary<long, BinaryDecisionDiagram>();
        private readonly Dictionary<BddPair, BinaryDecisionDiagram> _orCache = new Dictionary<BddPair, BinaryDecisionDiagram>();
        private readonly Dictionary<BddPair, BinaryDecisionDiagram> _andCache = new Dictionary<BddPair, BinaryDecisionDiagram>();
        private readonly Dictionary<int, BinaryDecisionDiagram> _intCache = new Dictionary<int, BinaryDecisionDiagram>();
        private readonly Dictionary<BinaryDecisionDiagram, BinaryDecisionDiagram> _notCache = new Dictionary<BinaryDecisionDiagram, BinaryDecisionDiagram>();
        private int _id = 2;
        private const int maxChar = ushort.MaxValue;
        private readonly int[] _bitOrder;
        private readonly int[] _bitMaps;

        public int NrOfBits { get; }

        public BddBuilder(Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            var k = encoding == Encoding.ASCII
                ? 7
                : encoding.IsSingleByte ? 8 : 16;

            _bitOrder = new int[k];
            _bitMaps = new int[k];
            for (int index = 0; index < k; ++index)
            {
                _bitOrder[index] = k - 1 - index;
                _bitMaps[index] = 1 << k - 1 - index;
            }
            NrOfBits = k;
        }

        public BddBuilder(int k)
        {
            _bitOrder = new int[k];
            _bitMaps = new int[k];
            for (int index = 0; index < k; ++index)
            {
                _bitOrder[index] = k - 1 - index;
                _bitMaps[index] = 1 << k - 1 - index;
            }
            NrOfBits = k;
        }

        private int MkId()
        {
            return _id++;
        }

        private static long MkRestrictKey(int v, bool makeTrue, BinaryDecisionDiagram bdd)
        {
            return ((long)bdd.Id << 16) + (v << 4) + (makeTrue ? 1L : 0L);
        }

        private static BddPair MkApplyKey(BinaryDecisionDiagram bdd1, BinaryDecisionDiagram bdd2)
        {
            return new BddPair(bdd1, bdd2);
        }

        private BinaryDecisionDiagram Restrict(int v, bool makeTrue, BinaryDecisionDiagram bdd)
        {
            long key = MkRestrictKey(v, makeTrue, bdd);
            if (_restrictCache.TryGetValue(key, out var bdd1))
                return bdd1;
            BinaryDecisionDiagram bdd2;
            if (v < bdd.Ordinal)
            {
                bdd2 = bdd;
            }
            else if (bdd.Ordinal < v)
            {
                BinaryDecisionDiagram t = Restrict(v, makeTrue, bdd.TrueCase);
                BinaryDecisionDiagram f = Restrict(v, makeTrue, bdd.FalseCase);
                bdd2 = f == t ? t : (f != bdd.FalseCase || t != bdd.TrueCase ? new BinaryDecisionDiagram(MkId(), bdd.Ordinal, t, f) : bdd);
            }
            else
            {
                bdd2 = makeTrue ? bdd.TrueCase : bdd.FalseCase;
            }

            _restrictCache[key] = bdd2;
            return bdd2;
        }

        public BinaryDecisionDiagram MkOr(BinaryDecisionDiagram constraint1, BinaryDecisionDiagram constraint2)
        {
            if (constraint1 == BinaryDecisionDiagram.False)
                return constraint2;
            if (constraint2 == BinaryDecisionDiagram.False)
                return constraint1;
            if (constraint1 == BinaryDecisionDiagram.True || constraint2 == BinaryDecisionDiagram.True)
                return BinaryDecisionDiagram.True;
            BddPair key = MkApplyKey(constraint1, constraint2);
            if (_orCache.TryGetValue(key, out var bdd))
                return bdd;
            if (constraint2.Ordinal < constraint1.Ordinal)
            {
                BinaryDecisionDiagram t = MkOr(constraint1, Restrict(constraint2.Ordinal, true, constraint2));
                BinaryDecisionDiagram f = MkOr(constraint1, Restrict(constraint2.Ordinal, false, constraint2));
                bdd = t == f ? t : new BinaryDecisionDiagram(MkId(), constraint2.Ordinal, t, f);
            }
            else if (constraint1.Ordinal < constraint2.Ordinal)
            {
                BinaryDecisionDiagram t = MkOr(Restrict(constraint1.Ordinal, true, constraint1), constraint2);
                BinaryDecisionDiagram f = MkOr(Restrict(constraint1.Ordinal, false, constraint1), constraint2);
                bdd = t == f ? t : new BinaryDecisionDiagram(MkId(), constraint1.Ordinal, t, f);
            }
            else
            {
                BinaryDecisionDiagram t = MkOr(Restrict(constraint1.Ordinal, true, constraint1), Restrict(constraint1.Ordinal, true, constraint2));
                BinaryDecisionDiagram f = MkOr(Restrict(constraint1.Ordinal, false, constraint1), Restrict(constraint1.Ordinal, false, constraint2));
                bdd = t == f ? t : new BinaryDecisionDiagram(MkId(), constraint1.Ordinal, t, f);
            }
            _orCache[key] = bdd;
            return bdd;
        }

        public BinaryDecisionDiagram MkAnd(BinaryDecisionDiagram constraint1, BinaryDecisionDiagram constraint2)
        {
            if (constraint1 == BinaryDecisionDiagram.True)
                return constraint2;
            if (constraint2 == BinaryDecisionDiagram.True)
                return constraint1;
            if (constraint1 == BinaryDecisionDiagram.False || constraint2 == BinaryDecisionDiagram.False)
                return BinaryDecisionDiagram.False;
            BddPair key = MkApplyKey(constraint1, constraint2);
            if (_andCache.TryGetValue(key, out var bdd))
                return bdd;
            if (constraint2.Ordinal < constraint1.Ordinal)
            {
                BinaryDecisionDiagram t = MkAnd(constraint1, Restrict(constraint2.Ordinal, true, constraint2));
                BinaryDecisionDiagram f = MkAnd(constraint1, Restrict(constraint2.Ordinal, false, constraint2));
                bdd = t == f ? t : new BinaryDecisionDiagram(MkId(), constraint2.Ordinal, t, f);
            }
            else if (constraint1.Ordinal < constraint2.Ordinal)
            {
                BinaryDecisionDiagram t = MkAnd(Restrict(constraint1.Ordinal, true, constraint1), constraint2);
                BinaryDecisionDiagram f = MkAnd(Restrict(constraint1.Ordinal, false, constraint1), constraint2);
                bdd = t == f ? t : new BinaryDecisionDiagram(MkId(), constraint1.Ordinal, t, f);
            }
            else
            {
                BinaryDecisionDiagram t = MkAnd(Restrict(constraint1.Ordinal, true, constraint1), Restrict(constraint1.Ordinal, true, constraint2));
                BinaryDecisionDiagram f = MkAnd(Restrict(constraint1.Ordinal, false, constraint1), Restrict(constraint1.Ordinal, false, constraint2));
                bdd = t == f ? t : new BinaryDecisionDiagram(MkId(), constraint1.Ordinal, t, f);
            }
            _andCache[key] = bdd;
            return bdd;
        }

        public BinaryDecisionDiagram MkNot(BinaryDecisionDiagram constraint)
        {
            if (constraint == BinaryDecisionDiagram.False)
                return BinaryDecisionDiagram.True;
            if (constraint == BinaryDecisionDiagram.True)
                return BinaryDecisionDiagram.False;
            if (_notCache.TryGetValue(constraint, out var bdd1))
                return bdd1;
            var bdd2 = new BinaryDecisionDiagram(MkId(), constraint.Ordinal, MkNot(constraint.TrueCase), MkNot(constraint.FalseCase));
            _notCache[constraint] = bdd2;
            return bdd2;
        }

        public BinaryDecisionDiagram MkAnd(IEnumerable<BinaryDecisionDiagram> constraints)
        {
            BinaryDecisionDiagram a = BinaryDecisionDiagram.True;
            foreach (BinaryDecisionDiagram condition in constraints)
                a = MkAnd(a, condition);
            return a;
        }

        public BinaryDecisionDiagram MkOr(IEnumerable<BinaryDecisionDiagram> constraints)
        {
            BinaryDecisionDiagram a = BinaryDecisionDiagram.False;
            foreach (BinaryDecisionDiagram condition in constraints)
                a = MkOr(a, condition);
            return a;
        }

        public BinaryDecisionDiagram True
        {
            get
            {
                return BinaryDecisionDiagram.True;
            }
        }

        public BinaryDecisionDiagram False
        {
            get
            {
                return BinaryDecisionDiagram.False;
            }
        }

        public BinaryDecisionDiagram MkBddForInt(int n)
        {
            if (_intCache.TryGetValue(n, out var bdd1))
                return bdd1;
            BinaryDecisionDiagram bdd2 = BinaryDecisionDiagram.True;
            for (int x = NrOfBits - 1; x >= 0; --x)
                bdd2 = (n & _bitMaps[x]) != 0 ? new BinaryDecisionDiagram(MkId(), x, bdd2, BinaryDecisionDiagram.False) : new BinaryDecisionDiagram(MkId(), x, BinaryDecisionDiagram.False, bdd2);
            _intCache[n] = bdd2;
            return bdd2;
        }

        public BinaryDecisionDiagram MkCharConstraint(bool caseInsensitive, char c)
        {
            if (caseInsensitive)
            {
                if (char.IsUpper(c))
                    return MkOr(MkBddForInt(c), MkBddForInt(char.ToLower(c)));
                if (char.IsLower(c))
                    return MkOr(MkBddForInt(c), MkBddForInt(char.ToUpper(c)));
            }
            return MkBddForInt(c);
        }

        public BinaryDecisionDiagram MkBddForIntRange(int m, int n)
        {
            BinaryDecisionDiagram a = BinaryDecisionDiagram.False;
            for (int n1 = m; n1 <= n; ++n1)
                a = MkOr(a, MkBddForInt(n1));
            return a;
        }

        public BinaryDecisionDiagram MkRangeConstraint(bool caseInsensitive, char lower, char upper)
        {
            if (NrOfBits == 7)
                return MkRangeConstraint1(caseInsensitive, lower < sbyte.MaxValue ? lower : '\x007F', upper < sbyte.MaxValue ? upper : '\x007F');
            if (NrOfBits == 8)
                return MkRangeConstraint1(caseInsensitive, lower < byte.MaxValue ? lower : 'ÿ', upper < byte.MaxValue ? upper : 'ÿ');
            var num1 = (int)lower;
            var num2 = (int)upper;
            if (num2 - num1 < ushort.MaxValue - num2 + num1 || caseInsensitive)
                return MkRangeConstraint1(caseInsensitive, lower, upper);
            return MkNot(MkOr(lower > 0 ? MkRangeConstraint1(caseInsensitive, char.MinValue, (char)(lower - 1U)) : BinaryDecisionDiagram.False, upper < ushort.MaxValue ? MkRangeConstraint1(caseInsensitive, (char)(upper + 1U), char.MaxValue) : BinaryDecisionDiagram.False));
        }

        public BinaryDecisionDiagram MkRangeConstraint1(bool ignoreCase, char c, char d)
        {
            BinaryDecisionDiagram a = BinaryDecisionDiagram.False;
            for (char c1 = c; c1 <= d; ++c1)
                a = MkOr(a, MkCharConstraint(ignoreCase, c1));
            return a;
        }

        public BinaryDecisionDiagram MkBddForIntRanges(IEnumerable<int[]> ranges)
        {
            BinaryDecisionDiagram a = BinaryDecisionDiagram.False;
            foreach (int[] range in ranges)
                a = MkOr(a, MkBddForIntRange(range[0], range[1]));
            return a;
        }

        public BinaryDecisionDiagram MkRangesConstraint(bool caseInsensitive, IEnumerable<char[]> ranges)
        {
            BinaryDecisionDiagram a = BinaryDecisionDiagram.False;
            foreach (char[] range in ranges)
                a = MkOr(a, MkRangeConstraint(caseInsensitive, range[0], range[1]));
            return a;
        }

        public char GenerateMember(Chooser chooser, BinaryDecisionDiagram bdd)
        {
            int num = 0;
            for (int index = 0; index < NrOfBits; ++index)
            {
                if (index < bdd.Ordinal)
                {
                    num |= chooser.ChooseBoolean() ? _bitMaps[index] : 0;
                }
                else if (bdd.FalseCase == BinaryDecisionDiagram.False)
                {
                    num |= _bitMaps[index];
                    bdd = bdd.TrueCase;
                }
                else if (bdd.TrueCase == BinaryDecisionDiagram.False)
                {
                    bdd = bdd.FalseCase;
                }
                else if (chooser.ChooseBoolean())
                {
                    num |= _bitMaps[index];
                    bdd = bdd.TrueCase;
                }
                else
                {
                    bdd = bdd.FalseCase;
                }
            }
            return (char)num;
        }

        private class BddPair : IEquatable<BddPair>
        {
            internal BddPair(BinaryDecisionDiagram a, BinaryDecisionDiagram b)
            {
                First = a;
                Second = b;
            }

            public BinaryDecisionDiagram First { get; }

            public BinaryDecisionDiagram Second { get; }

            public override int GetHashCode() => First.Id + (Second.Id << 1);

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(obj, null))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                return Equals(obj as BddPair);
            }

            public bool Equals(BddPair other)
            {
                if (ReferenceEquals(other, null))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return First == other.First
                    && Second == other.Second;
            }
        }
    }
}
