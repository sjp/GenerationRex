using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Rex
{
    public class RexEngine
    {
        private const int tryLimitMin = 100;
        private const int tryLimitMax = 200;
        private Chooser chooser;
        private BinaryDecisionDiagramBuilder solver;
        private RegexToSFA<BinaryDecisionDiagram> converter;

        internal RexEngine(BinaryDecisionDiagramBuilder solver)
        {
            this.solver = solver;
            chooser = new Chooser();
            converter = new RegexToSFA<BinaryDecisionDiagram>(solver, new UnicodeCategoryConditionsBddProvider(solver));
        }

        public RexEngine(CharacterEncoding encoding, int randomSeed)
        {
            solver = new BinaryDecisionDiagramBuilder((int)encoding);
            chooser = new Chooser();
            if (randomSeed > -1)
                chooser.RandomSeed = randomSeed;
            converter = new RegexToSFA<BinaryDecisionDiagram>(solver, new UnicodeCategoryConditionsBddProvider(solver));
        }

        public int RandomSeed
        {
            get
            {
                return chooser.RandomSeed;
            }
        }

        internal string GenerateMember(SymbolicFiniteAutomaton<BinaryDecisionDiagram> fa)
        {
            var stringBuilder = new StringBuilder();
            Move<BinaryDecisionDiagram> nthMoveFrom;
            for (int index = fa.InitialState; !fa.IsFinalState(index) || fa.OutDegree(index) > 0 && chooser.ChooseBoolean(); index = nthMoveFrom.TargetState)
            {
                nthMoveFrom = fa.GetNthMoveFrom(index, chooser.Choose(fa.GetMovesCountFrom(index)));
                if (!nthMoveFrom.IsEpsilon)
                    stringBuilder.Append(solver.GenerateMember(chooser, nthMoveFrom.Condition));
            }
            return stringBuilder.ToString();
        }

        public IEnumerable<string> GenerateMembers(RegexOptions options, int k, params string[] regexes)
        {
            return GenerateMembers(CreateSfaFromRegexes(options, regexes), k);
        }

        internal IEnumerable<string> GenerateMembers(SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa, int k)
        {
            if (sfa != null && !sfa.IsEmpty)
            {
                var old = new HashSet<string>();
                for (int i = 0; i < k; ++i)
                {
                    string member = GenerateMember(sfa);
                    int tryCount = Math.Min(100 + old.Count, 200);
                    while (old.Contains(member) && tryCount-- > 0)
                        member = GenerateMember(sfa);
                    if (tryCount < 0 && old.Contains(member))
                        break;
                    old.Add(member);
                    yield return member;
                }
            }
        }

        internal SymbolicFiniteAutomaton<BinaryDecisionDiagram> CreateSfaFromRegexes(RegexOptions options, params string[] regexes)
        {
            SymbolicFiniteAutomaton<BinaryDecisionDiagram> a = null;
            foreach (string regex in regexes)
            {
                SymbolicFiniteAutomaton<BinaryDecisionDiagram> b1 = converter.Convert(regex, options);
                a = a == null ? b1 : SymbolicFiniteAutomaton<BinaryDecisionDiagram>.MkProduct(a, b1, new Func<BinaryDecisionDiagram, BinaryDecisionDiagram, BinaryDecisionDiagram>(solver.MkAnd), new Func<BinaryDecisionDiagram, BinaryDecisionDiagram, BinaryDecisionDiagram>(solver.MkOr), b => b != solver.False);
                if (a.IsEmpty)
                    break;
            }
            return a;
        }

        public static IEnumerable<string> GenerateMembers(RexSettings settings)
        {
            var rexEngine = new RexEngine(settings.encoding, settings.seed);
            var options = RegexOptions.None;
            if (settings.options != null)
            {
                foreach (RegexOptions option in settings.options)
                    options |= option;
            }
            return rexEngine.GenerateMembers(options, settings.k, settings.regexes);
        }

        public static string Escape(char c)
        {
            int i = c;
            if (i > sbyte.MaxValue)
                return ToUnicodeRepr(i);
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
            string str = string.Format("{0:X}", i);
            return str.Length != 1 ? (str.Length != 2 ? (str.Length != 3 ? "\\u" + str : "\\u0" + str) : "\\u00" + str) : "\\u000" + str;
        }

        public static string Escape(string s)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("\"");
            foreach (char c in s)
                stringBuilder.Append(Escape(c));
            stringBuilder.Append("\"");
            return stringBuilder.ToString();
        }

        internal void ToDot(TextWriter dot, SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa)
        {
            converter.ToDot(sfa, "SFA", dot, DotRankDir.LR, 12);
        }
    }
}
