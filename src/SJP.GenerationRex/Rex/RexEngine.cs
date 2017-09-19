using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SJP.GenerationRex
{
    public class RexEngine
    {
        internal RexEngine(BddBuilder solver)
        {
            _solver = solver;
            _chooser = new Chooser();
            _converter = new RegexToSFA<BinaryDecisionDiagram>(solver, new UnicodeCategoryConditionsBddProvider(Encoding.Unicode, solver.NrOfBits));
        }

        public RexEngine(CharacterEncoding encoding, int randomSeed)
        {
            _solver = new BddBuilder(encoding);
            _chooser = new Chooser();
            if (randomSeed > -1)
                _chooser.RandomSeed = randomSeed;

            _converter = new RegexToSFA<BinaryDecisionDiagram>(_solver, new UnicodeCategoryConditionsBddProvider(Encoding.Unicode, _solver.NrOfBits));
        }

        public RexEngine(Encoding encoding, int randomSeed)
        {
            _solver = new BddBuilder(encoding);
            _chooser = new Chooser();
            if (randomSeed > -1)
                _chooser.RandomSeed = randomSeed;

            _converter = new RegexToSFA<BinaryDecisionDiagram>(_solver, new UnicodeCategoryConditionsBddProvider(encoding, _solver.NrOfBits));
        }

        public int RandomSeed => _chooser.RandomSeed;

        internal string GenerateMember(SymbolicFiniteAutomaton<BinaryDecisionDiagram> fa)
        {
            var stringBuilder = new StringBuilder();
            Move<BinaryDecisionDiagram> nthMoveFrom;
            for (int index = fa.InitialState; !fa.IsFinalState(index) || (fa.OutDegree(index) > 0 && _chooser.ChooseBoolean()); index = nthMoveFrom.TargetState)
            {
                nthMoveFrom = fa.GetNthMoveFrom(index, this._chooser.Choose(fa.GetMovesCountFrom(index)));
                if (!nthMoveFrom.IsEpsilon)
                    stringBuilder.Append(this._solver.GenerateMember(this._chooser, nthMoveFrom.Condition));
            }
            return stringBuilder.ToString();
        }

        public IEnumerable<string> GenerateMembers(RegexOptions options, int k, params string[] regexes)
        {
            return GenerateMembers(CreateSFAFromRegexes(options, regexes), k);
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

        internal SymbolicFiniteAutomaton<BinaryDecisionDiagram> CreateSFAFromRegexes(RegexOptions options, params string[] regexes)
        {
            SymbolicFiniteAutomaton<BinaryDecisionDiagram> a = null;
            foreach (string regex in regexes)
            {
                SymbolicFiniteAutomaton<BinaryDecisionDiagram> b1 = _converter.Convert(regex, options);
                a = a == null ? b1 : SymbolicFiniteAutomaton<BinaryDecisionDiagram>.MkProduct(a, b1,  _solver.MkAnd, _solver.MkOr, b => b != _solver.False);
                if (a.IsEmpty)
                    break;
            }
            return a;
        }

        public static IEnumerable<string> GenerateMembers(RexSettings settings)
        {
            var rexEngine = new RexEngine(Encoding.Unicode, settings.seed);
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
            int i = (int)c;
            if (i > (int)sbyte.MaxValue)
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
            var str = string.Format("{0:X}", i);
            return str.Length != 1
                ? (str.Length != 2
                    ? (str.Length != 3
                        ? "\\u" + str
                        : "\\u0" + str)
                    : "\\u00" + str)
                : "\\u000" + str;
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

        internal void ToDot(TextWriter dot, SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa)
        {
            _converter.ToDot(sfa, "SFA", dot, DotRankDir.LR, 12);
        }

        private const int TryLimitMin = 100;
        private const int TryLimitMax = 200;
        private readonly Chooser _chooser;
        private readonly BddBuilder _solver;
        private readonly RegexToSFA<BinaryDecisionDiagram> _converter;
    }
}
