using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SJP.GenerationRex
{
    public class RexEngine
    {
        private const int tryLimitMin = 100;
        private const int tryLimitMax = 200;
        private Chooser chooser;
        private BddBuilder solver;
        private RegexToSFA<BDD> converter;

        internal RexEngine(BddBuilder solver)
        {
            this.solver = solver;
            this.chooser = new Chooser();
            this.converter = new RegexToSFA<BDD>((ICharacterConstraintSolver<BDD>)solver, (IUnicodeCategoryConditions<BDD>)new UnicodeCategoryConditionsBddProvider(solver));
        }

        public RexEngine(CharacterEncoding encoding, int randomSeed)
        {
            this.solver = new BddBuilder((int)encoding);
            this.chooser = new Chooser();
            if (randomSeed > -1)
                this.chooser.RandomSeed = randomSeed;
            this.converter = new RegexToSFA<BDD>((ICharacterConstraintSolver<BDD>)this.solver, (IUnicodeCategoryConditions<BDD>)new UnicodeCategoryConditionsBddProvider(this.solver));
        }

        public int RandomSeed
        {
            get
            {
                return this.chooser.RandomSeed;
            }
        }

        internal string GenerateMember(SFA<BDD> fa)
        {
            StringBuilder stringBuilder = new StringBuilder();
            MOVE<BDD> nthMoveFrom;
            for (int index = fa.InitialState; !fa.IsFinalState(index) || fa.OutDegree(index) > 0 && this.chooser.ChooseTrueOrFalse(); index = nthMoveFrom.TargetState)
            {
                nthMoveFrom = fa.GetNthMoveFrom(index, this.chooser.Choose(fa.GetMovesCountFrom(index)));
                if (!nthMoveFrom.IsEpsilon)
                    stringBuilder.Append(this.solver.GenerateMember(this.chooser, nthMoveFrom.Condition));
            }
            return stringBuilder.ToString();
        }

        public IEnumerable<string> GenerateMembers(RegexOptions options, int k, params string[] regexes)
        {
            return this.GenerateMembers(this.CreateSFAFromRegexes(options, regexes), k);
        }

        internal IEnumerable<string> GenerateMembers(SFA<BDD> sfa, int k)
        {
            if (sfa != null && !sfa.IsEmpty)
            {
                HashSet<string> old = new HashSet<string>();
                for (int i = 0; i < k; ++i)
                {
                    string member = this.GenerateMember(sfa);
                    int tryCount = Math.Min(100 + old.Count, 200);
                    while (old.Contains(member) && tryCount-- > 0)
                        member = this.GenerateMember(sfa);
                    if (tryCount < 0 && old.Contains(member))
                        break;
                    old.Add(member);
                    yield return member;
                }
            }
        }

        internal SFA<BDD> CreateSFAFromRegexes(RegexOptions options, params string[] regexes)
        {
            SFA<BDD> a = (SFA<BDD>)null;
            foreach (string regex in regexes)
            {
                SFA<BDD> b1 = this.converter.Convert(regex, options);
                a = a == null ? b1 : SFA<BDD>.MkProduct(a, b1, new Func<BDD, BDD, BDD>(this.solver.MkAnd), new Func<BDD, BDD, BDD>(this.solver.MkOr), (Func<BDD, bool>)(b => b != this.solver.False));
                if (a.IsEmpty)
                    break;
            }
            return a;
        }

        public static IEnumerable<string> GenerateMembers(RexSettings settings)
        {
            RexEngine rexEngine = new RexEngine(settings.encoding, settings.seed);
            RegexOptions options = RegexOptions.None;
            if (settings.options != null)
            {
                foreach (RegexOptions option in settings.options)
                    options |= option;
            }
            return rexEngine.GenerateMembers(options, settings.k, settings.regexes);
        }

        public static string Escape(char c)
        {
            int i = (int)c;
            if (i > (int)sbyte.MaxValue)
                return RexEngine.ToUnicodeRepr(i);
            switch (c)
            {
                case char.MinValue:
                    return "\\0";
                case '\a':
                    return "\\a";
                case '\b':
                    return "\\b";
                case '\t':
                    return "\\t";
                case '\n':
                    return "\\n";
                case '\v':
                    return "\\v";
                case '\f':
                    return "\\f";
                case '\r':
                    return "\\r";
                case '\x001B':
                    return "\\e";
                case '"':
                    return "\\\"";
                default:
                    return c.ToString();
            }
        }

        private static string ToUnicodeRepr(int i)
        {
            string str = string.Format("{0:X}", (object)i);
            return str.Length != 1 ? (str.Length != 2 ? (str.Length != 3 ? "\\u" + str : "\\u0" + str) : "\\u00" + str) : "\\u000" + str;
        }

        public static string Escape(string s)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\"");
            foreach (char c in s)
                stringBuilder.Append(RexEngine.Escape(c));
            stringBuilder.Append("\"");
            return stringBuilder.ToString();
        }

        internal void ToDot(TextWriter dot, SFA<BDD> sfa)
        {
            this.converter.ToDot(sfa, "SFA", dot, DotRankDir.LR, 12);
        }
    }
}
