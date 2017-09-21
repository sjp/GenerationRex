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

        public RexEngine(Encoding encoding, int randomSeed)
        {
            _solver = new BddBuilder(encoding);
            _chooser = new Chooser();
            if (randomSeed > -1)
                _chooser.RandomSeed = randomSeed;

            _converter = new RegexToSFA<BinaryDecisionDiagram>(_solver, new UnicodeCategoryConditionsBddProvider(encoding, _solver.NrOfBits));
        }

        // TODO:
        // Make sure that the following behaviour can be integrated:
        // - enumerable of regexes
        // - count of regexes to generate (or infinite?)
        // - produce an intersection of regular expressions? might not be useful though
        // - provide encoding
        // - provide seed for deterministic generation of regexes

        public int RandomSeed => _chooser.RandomSeed;

        internal string GenerateMember(SymbolicFiniteAutomaton<BinaryDecisionDiagram> fa)
        {
            var stringBuilder = new StringBuilder();
            Move<BinaryDecisionDiagram> nthMoveFrom;
            for (int index = fa.InitialState; !fa.IsFinalState(index) || (fa.OutDegree(index) > 0 && _chooser.ChooseBoolean()); index = nthMoveFrom.TargetState)
            {
                nthMoveFrom = fa.GetNthMoveFrom(index, _chooser.Choose(fa.GetMovesCountFrom(index)));
                if (!nthMoveFrom.IsEpsilon)
                    stringBuilder.Append(_solver.GenerateMember(_chooser, nthMoveFrom.Condition));
            }
            return stringBuilder.ToString();
        }

        public IEnumerable<string> GenerateMembers(RegexOptions options, int k, params string[] regexes)
        {
            return GenerateMembers(CreateSFAFromRegexes(options, regexes), k);
        }

        internal IEnumerable<string> GenerateMembers(SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa, int k)
        {
            if (sfa == null)
                throw new ArgumentNullException(nameof(sfa));
            if (sfa.IsEmpty)
                throw new ArgumentException("Cannot generate a member for an empty state machine.", nameof(sfa));
            if (k < 0)
                throw new ArgumentOutOfRangeException(nameof(k), "The number of values to generate must be non-negative.");

            var generatedValues = new HashSet<string>();
            for (var i = 0; i < k; ++i)
            {
                string member = GenerateMember(sfa);
                int tryCount = Math.Min(100 + generatedValues.Count, 200);
                while (generatedValues.Contains(member) && tryCount-- > 0)
                    member = GenerateMember(sfa);
                if (tryCount < 0 && generatedValues.Contains(member))
                    break;
                generatedValues.Add(member);
                yield return member;
            }
        }

        // TODO RENAME TO INTERSECTION AS THIS IS AN INTERSECTION OF REGEXES
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

        private const int TryLimitMin = 100;
        private const int TryLimitMax = 200;
        private readonly Chooser _chooser;
        private readonly BddBuilder _solver;
        private readonly RegexToSFA<BinaryDecisionDiagram> _converter;
    }
}
