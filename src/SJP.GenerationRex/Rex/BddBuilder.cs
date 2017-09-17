using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SJP.GenerationRex
{
    internal class BddBuilder : ICharacterConstraintSolver<BDD>
    {
        private Dictionary<long, BDD> restrictCache = new Dictionary<long, BDD>();
        private Dictionary<BddBuilder.BddPair, BDD> orCache = new Dictionary<BddBuilder.BddPair, BDD>();
        private Dictionary<BddBuilder.BddPair, BDD> andCache = new Dictionary<BddBuilder.BddPair, BDD>();
        private Dictionary<int, BDD> intCache = new Dictionary<int, BDD>();
        private Dictionary<BDD, BDD> notCache = new Dictionary<BDD, BDD>();
        private int id = 2;
        private const int maxChar = 65535;
        private int[] bitOrder;
        private int[] bitMaps;
        private int k;

        public int NrOfBits
        {
            get
            {
                return this.k;
            }
        }

        public BddBuilder(int k)
        {
            this.bitOrder = new int[k];
            this.bitMaps = new int[k];
            for (int index = 0; index < k; ++index)
            {
                this.bitOrder[index] = k - 1 - index;
                this.bitMaps[index] = 1 << k - 1 - index;
            }
            this.k = k;
        }

        public BddBuilder(int k, int randomSeed)
        {
            this.bitOrder = new int[k];
            this.bitMaps = new int[k];
            for (int index = 0; index < k; ++index)
            {
                this.bitOrder[index] = k - 1 - index;
                this.bitMaps[index] = 1 << k - 1 - index;
            }
            this.k = k;
        }

        private int MkId()
        {
            return this.id++;
        }

        private static long MkRestrictKey(int v, bool makeTrue, BDD bdd)
        {
            return ((long)bdd.Id << 16) + (long)(v << 4) + (makeTrue ? 1L : 0L);
        }

        private static BddBuilder.BddPair MkApplyKey(BDD bdd1, BDD bdd2)
        {
            return new BddBuilder.BddPair(bdd1, bdd2);
        }

        private BDD Restrict(int v, bool makeTrue, BDD bdd)
        {
            long key = BddBuilder.MkRestrictKey(v, makeTrue, bdd);
            BDD bdd1;
            if (this.restrictCache.TryGetValue(key, out bdd1))
                return bdd1;
            BDD bdd2;
            if (v < bdd.Ordinal)
                bdd2 = bdd;
            else if (bdd.Ordinal < v)
            {
                BDD t = this.Restrict(v, makeTrue, bdd.T);
                BDD f = this.Restrict(v, makeTrue, bdd.F);
                bdd2 = f == t ? t : (f != bdd.F || t != bdd.T ? new BDD(this.MkId(), bdd.Ordinal, t, f) : bdd);
            }
            else
                bdd2 = makeTrue ? bdd.T : bdd.F;
            this.restrictCache[key] = bdd2;
            return bdd2;
        }

        public BDD MkOr(BDD constraint1, BDD constraint2)
        {
            if (constraint1 == BDD.False)
                return constraint2;
            if (constraint2 == BDD.False)
                return constraint1;
            if (constraint1 == BDD.True || constraint2 == BDD.True)
                return BDD.True;
            BddBuilder.BddPair key = BddBuilder.MkApplyKey(constraint1, constraint2);
            BDD bdd;
            if (this.orCache.TryGetValue(key, out bdd))
                return bdd;
            if (constraint2.Ordinal < constraint1.Ordinal)
            {
                BDD t = this.MkOr(constraint1, this.Restrict(constraint2.Ordinal, true, constraint2));
                BDD f = this.MkOr(constraint1, this.Restrict(constraint2.Ordinal, false, constraint2));
                bdd = t == f ? t : new BDD(this.MkId(), constraint2.Ordinal, t, f);
            }
            else if (constraint1.Ordinal < constraint2.Ordinal)
            {
                BDD t = this.MkOr(this.Restrict(constraint1.Ordinal, true, constraint1), constraint2);
                BDD f = this.MkOr(this.Restrict(constraint1.Ordinal, false, constraint1), constraint2);
                bdd = t == f ? t : new BDD(this.MkId(), constraint1.Ordinal, t, f);
            }
            else
            {
                BDD t = this.MkOr(this.Restrict(constraint1.Ordinal, true, constraint1), this.Restrict(constraint1.Ordinal, true, constraint2));
                BDD f = this.MkOr(this.Restrict(constraint1.Ordinal, false, constraint1), this.Restrict(constraint1.Ordinal, false, constraint2));
                bdd = t == f ? t : new BDD(this.MkId(), constraint1.Ordinal, t, f);
            }
            this.orCache[key] = bdd;
            return bdd;
        }

        public BDD MkAnd(BDD constraint1, BDD constraint2)
        {
            if (constraint1 == BDD.True)
                return constraint2;
            if (constraint2 == BDD.True)
                return constraint1;
            if (constraint1 == BDD.False || constraint2 == BDD.False)
                return BDD.False;
            BddBuilder.BddPair key = BddBuilder.MkApplyKey(constraint1, constraint2);
            BDD bdd;
            if (this.andCache.TryGetValue(key, out bdd))
                return bdd;
            if (constraint2.Ordinal < constraint1.Ordinal)
            {
                BDD t = this.MkAnd(constraint1, this.Restrict(constraint2.Ordinal, true, constraint2));
                BDD f = this.MkAnd(constraint1, this.Restrict(constraint2.Ordinal, false, constraint2));
                bdd = t == f ? t : new BDD(this.MkId(), constraint2.Ordinal, t, f);
            }
            else if (constraint1.Ordinal < constraint2.Ordinal)
            {
                BDD t = this.MkAnd(this.Restrict(constraint1.Ordinal, true, constraint1), constraint2);
                BDD f = this.MkAnd(this.Restrict(constraint1.Ordinal, false, constraint1), constraint2);
                bdd = t == f ? t : new BDD(this.MkId(), constraint1.Ordinal, t, f);
            }
            else
            {
                BDD t = this.MkAnd(this.Restrict(constraint1.Ordinal, true, constraint1), this.Restrict(constraint1.Ordinal, true, constraint2));
                BDD f = this.MkAnd(this.Restrict(constraint1.Ordinal, false, constraint1), this.Restrict(constraint1.Ordinal, false, constraint2));
                bdd = t == f ? t : new BDD(this.MkId(), constraint1.Ordinal, t, f);
            }
            this.andCache[key] = bdd;
            return bdd;
        }

        public BDD MkNot(BDD constraint)
        {
            if (constraint == BDD.False)
                return BDD.True;
            if (constraint == BDD.True)
                return BDD.False;
            BDD bdd1;
            if (this.notCache.TryGetValue(constraint, out bdd1))
                return bdd1;
            BDD bdd2 = new BDD(this.MkId(), constraint.Ordinal, this.MkNot(constraint.T), this.MkNot(constraint.F));
            this.notCache[constraint] = bdd2;
            return bdd2;
        }

        public BDD MkAnd(IEnumerable<BDD> constraints)
        {
            BDD a = BDD.True;
            foreach (BDD condition in constraints)
                a = this.MkAnd(a, condition);
            return a;
        }

        public BDD MkOr(IEnumerable<BDD> constraints)
        {
            BDD a = BDD.False;
            foreach (BDD condition in constraints)
                a = this.MkOr(a, condition);
            return a;
        }

        public BDD True
        {
            get
            {
                return BDD.True;
            }
        }

        public BDD False
        {
            get
            {
                return BDD.False;
            }
        }

        public BDD MkBddForInt(int n)
        {
            BDD bdd1;
            if (this.intCache.TryGetValue(n, out bdd1))
                return bdd1;
            BDD bdd2 = BDD.True;
            for (int x = this.k - 1; x >= 0; --x)
                bdd2 = (n & this.bitMaps[x]) != 0 ? new BDD(this.MkId(), x, bdd2, BDD.False) : new BDD(this.MkId(), x, BDD.False, bdd2);
            this.intCache[n] = bdd2;
            return bdd2;
        }

        public BDD MkCharConstraint(bool caseInsensitive, char c)
        {
            if (caseInsensitive)
            {
                if (char.IsUpper(c))
                    return this.MkOr(this.MkBddForInt((int)c), this.MkBddForInt((int)char.ToLower(c)));
                if (char.IsLower(c))
                    return this.MkOr(this.MkBddForInt((int)c), this.MkBddForInt((int)char.ToUpper(c)));
            }
            return this.MkBddForInt((int)c);
        }

        public BDD MkBddForIntRange(int m, int n)
        {
            BDD a = BDD.False;
            for (int n1 = m; n1 <= n; ++n1)
                a = this.MkOr(a, this.MkBddForInt(n1));
            return a;
        }

        public BDD MkRangeConstraint(bool caseInsensitive, char lower, char upper)
        {
            if (this.k == 7)
                return this.MkRangeConstraint1(caseInsensitive, (int)lower < (int)sbyte.MaxValue ? lower : '\x007F', (int)upper < (int)sbyte.MaxValue ? upper : '\x007F');
            if (this.k == 8)
                return this.MkRangeConstraint1(caseInsensitive, (int)lower < (int)byte.MaxValue ? lower : 'ÿ', (int)upper < (int)byte.MaxValue ? upper : 'ÿ');
            int num1 = (int)lower;
            int num2 = (int)upper;
            if (num2 - num1 < (int)ushort.MaxValue - num2 + num1 || caseInsensitive)
                return this.MkRangeConstraint1(caseInsensitive, lower, upper);
            return this.MkNot(this.MkOr((int)lower > 0 ? this.MkRangeConstraint1(caseInsensitive, char.MinValue, (char)((uint)lower - 1U)) : BDD.False, (int)upper < (int)ushort.MaxValue ? this.MkRangeConstraint1(caseInsensitive, (char)((uint)upper + 1U), char.MaxValue) : BDD.False));
        }

        public BDD MkRangeConstraint1(bool ignoreCase, char c, char d)
        {
            BDD a = BDD.False;
            for (char c1 = c; (int)c1 <= (int)d; ++c1)
                a = this.MkOr(a, this.MkCharConstraint(ignoreCase, c1));
            return a;
        }

        public BDD MkBddForIntRanges(IEnumerable<int[]> ranges)
        {
            BDD a = BDD.False;
            foreach (int[] range in ranges)
                a = this.MkOr(a, this.MkBddForIntRange(range[0], range[1]));
            return a;
        }

        public BDD MkRangesConstraint(bool caseInsensitive, IEnumerable<char[]> ranges)
        {
            BDD a = BDD.False;
            foreach (char[] range in ranges)
                a = this.MkOr(a, this.MkRangeConstraint(caseInsensitive, range[0], range[1]));
            return a;
        }

        public char GenerateMember(Chooser chooser, BDD bdd)
        {
            int num = 0;
            for (int index = 0; index < this.k; ++index)
            {
                if (index < bdd.Ordinal)
                    num |= chooser.ChooseTrueOrFalse() ? this.bitMaps[index] : 0;
                else if (bdd.F == BDD.False)
                {
                    num |= this.bitMaps[index];
                    bdd = bdd.T;
                }
                else if (bdd.T == BDD.False)
                    bdd = bdd.F;
                else if (chooser.ChooseTrueOrFalse())
                {
                    num |= this.bitMaps[index];
                    bdd = bdd.T;
                }
                else
                    bdd = bdd.F;
            }
            return (char)num;
        }

        public IEnumerable<int[]> Serialize(BDD bdd)
        {
            HashSet<BDD> done = new HashSet<BDD>();
            Stack<BDD> stack = new Stack<BDD>();
            if (bdd.Id > 1)
                stack.Push(bdd);
            while (stack.Count > 0)
            {
                BDD b = stack.Pop();
                yield return new int[4]
                {
          b.Ordinal,
          b.Id,
          b.T.Id,
          b.F.Id
                };
                if (b.T.Id > 1 && !done.Contains(b.T))
                {
                    done.Add(b.T);
                    stack.Push(b.T);
                }
                if (b.F.Id > 1 && !done.Contains(b.F))
                {
                    done.Add(b.F);
                    stack.Push(b.F);
                }
            }
        }

        public int[] SerializeCompact(BDD bdd)
        {
            if (bdd == BDD.False)
                return new int[1];
            if (bdd == BDD.True)
                return new int[2];
            int[] numArray = new int[bdd.CalculateSize()];
            Dictionary<BDD, int> dictionary = new Dictionary<BDD, int>();
            dictionary[BDD.False] = 0;
            dictionary[BDD.True] = 1;
            Stack<BDD> bddStack = new Stack<BDD>();
            bddStack.Push(bdd);
            dictionary[bdd] = 2;
            int num1 = 3;
            while (bddStack.Count > 0)
            {
                BDD index1 = bddStack.Pop();
                if (!dictionary.ContainsKey(index1.T))
                {
                    dictionary[index1.T] = num1++;
                    bddStack.Push(index1.T);
                }
                if (!dictionary.ContainsKey(index1.F))
                {
                    dictionary[index1.F] = num1++;
                    bddStack.Push(index1.F);
                }
                int index2 = dictionary[index1];
                int num2 = dictionary[index1.F];
                int num3 = dictionary[index1.T];
                numArray[index2] = index1.Ordinal << 28 | num3 << 14 | num2;
            }
            return numArray;
        }

        public BDD Deserialize(IEnumerable<int[]> arcs)
        {
            BDD bdd1 = (BDD)null;
            Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
            Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
            Dictionary<int, BDD> dictionary3 = new Dictionary<int, BDD>();
            foreach (int[] arc in arcs)
            {
                BDD bdd2 = new BDD(this.MkId(), arc[0]);
                dictionary3[arc[1]] = bdd2;
                dictionary1[arc[1]] = arc[2];
                dictionary2[arc[1]] = arc[3];
                if (bdd1 == null)
                    bdd1 = bdd2;
            }
            foreach (int key in dictionary3.Keys)
            {
                int index1 = dictionary1[key];
                int index2 = dictionary2[key];
                dictionary3[key].T = index1 == 0 ? BDD.False : (index1 == 1 ? BDD.True : dictionary3[index1]);
                dictionary3[key].F = index2 == 0 ? BDD.False : (index2 == 1 ? BDD.True : dictionary3[index2]);
            }
            return bdd1 ?? BDD.True;
        }

        public BDD DeserializeCompact(int[] arcs)
        {
            if (arcs.Length == 1)
                return BDD.False;
            if (arcs.Length == 2)
                return BDD.True;
            Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
            Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
            BDD[] bddArray = new BDD[arcs.Length];
            bddArray[0] = BDD.False;
            bddArray[1] = BDD.True;
            for (int index = 2; index < arcs.Length; ++index)
            {
                int x = arcs[index] >> 28 & 15;
                int num1 = arcs[index] >> 14 & 16383;
                int num2 = arcs[index] & 16383;
                BDD bdd = new BDD(this.MkId(), x);
                bddArray[index] = bdd;
                dictionary1[index] = num1;
                dictionary2[index] = num2;
            }
            for (int index1 = 2; index1 < bddArray.Length; ++index1)
            {
                int index2 = dictionary1[index1];
                int index3 = dictionary2[index1];
                bddArray[index1].T = bddArray[index2];
                bddArray[index1].F = bddArray[index3];
            }
            return bddArray[2];
        }

        public static void Display(BDD bdd, string name, DotRankDir dir, int fontsize, bool showgraph, string format)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string str = string.Format("{1}\\{0}.dot", (object)name, (object)currentDirectory);
            string fileName = string.Format("{2}\\{0}.{1}", (object)name, (object)format, (object)currentDirectory);
            FileInfo fileInfo1 = new FileInfo(str);
            if (fileInfo1.Exists)
                fileInfo1.IsReadOnly = false;
            FileInfo fileInfo2 = new FileInfo(fileName);
            if (fileInfo2.Exists)
                fileInfo2.IsReadOnly = false;
            BddBuilder.ToDot(bdd, name, str, dir, fontsize);
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo("dot.exe", string.Format("-T{2} {0} -o {1}", (object)str, (object)fileName, (object)format));
            try
            {
                process.Start();
                process.WaitForExit();
                if (!showgraph)
                    return;
                process.StartInfo = new ProcessStartInfo(fileName);
                process.Start();
            }
            catch (Exception ex)
            {
                throw new RexException("Dot viewer is not installed", ex);
            }
        }

        public static void ToDot(BDD bdd, string bddName, string filename, DotRankDir rankdir, int fontsize)
        {
            StreamWriter tw = new StreamWriter(filename);
            BddBuilder.ToDot(bdd, bddName, tw, rankdir, fontsize);
            tw.Close();
        }

        public static void ToDot(BDD bdd, string bddName, StreamWriter tw, DotRankDir rankdir, int fontsize)
        {
            if (bdd.Id < 2)
                throw new ArgumentOutOfRangeException(nameof(bdd), "Must be different from BDD.True and BDD.False");
            Dictionary<BDD, int> dictionary1 = new Dictionary<BDD, int>();
            Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
            List<MOVE<string>> moveList = new List<MOVE<string>>();
            Stack<BDD> bddStack = new Stack<BDD>();
            bddStack.Push(bdd);
            dictionary1.Add(BDD.False, 0);
            dictionary1.Add(BDD.True, 1);
            dictionary1.Add(bdd, 2);
            int num = 3;
            int val2 = 0;
            while (bddStack.Count > 0)
            {
                BDD index = bddStack.Pop();
                int sourceState = dictionary1[index];
                dictionary2[sourceState] = index.Ordinal;
                val2 = Math.Max(index.Ordinal, val2);
                if (!dictionary1.ContainsKey(index.F))
                {
                    dictionary1[index.F] = num++;
                    bddStack.Push(index.F);
                }
                if (!dictionary1.ContainsKey(index.T))
                {
                    dictionary1[index.T] = num++;
                    bddStack.Push(index.T);
                }
                moveList.Add(MOVE<string>.T(sourceState, dictionary1[index.F], "0"));
                moveList.Add(MOVE<string>.T(sourceState, dictionary1[index.T], "1"));
            }
            dictionary2[0] = val2 + 1;
            dictionary2[1] = val2 + 1;
            tw.WriteLine("digraph \"" + bddName + "\" {");
            tw.WriteLine(string.Format("rankdir={0};", (object)rankdir.ToString()));
            tw.WriteLine();
            tw.WriteLine("//Nodes");
            tw.WriteLine(string.Format("node [style = filled, shape = ellipse, peripheries = 1, fillcolor = white, fontsize = {0}]", (object)fontsize));
            foreach (int key in dictionary2.Keys)
            {
                if (key > 1)
                    tw.WriteLine("{0} [label = {1}, group = {1}]", (object)key, (object)dictionary2[key]);
            }
            tw.WriteLine("//True and False");
            tw.WriteLine(string.Format("node [style = filled, shape= polygon, sides=4, fillcolor = white, fontsize = {0}]", (object)fontsize));
            tw.WriteLine("0 [label = False, group = {0}]", (object)val2);
            tw.WriteLine("1 [label = True, group = {0}]", (object)val2);
            tw.WriteLine();
            tw.WriteLine("//Links");
            foreach (MOVE<string> move in moveList)
                tw.WriteLine(string.Format("{0} -> {1} [label = \"{2}\", fontsize = {3} ];", (object)move.SourceState, (object)move.TargetState, (object)move.Condition, (object)fontsize));
            tw.WriteLine("}");
        }

        private class BddPair
        {
            private BDD a;
            private BDD b;

            internal BddPair(BDD a, BDD b)
            {
                this.a = a;
                this.b = b;
            }

            public override int GetHashCode()
            {
                return this.a.Id + (this.b.Id << 1);
            }

            public override bool Equals(object obj)
            {
                BddBuilder.BddPair bddPair = (BddBuilder.BddPair)obj;
                if (bddPair.a == this.a)
                    return bddPair.b == this.b;
                return false;
            }
        }
    }
}
