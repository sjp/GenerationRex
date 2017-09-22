using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EnumsNET;

namespace SJP.GenerationRex
{
    public class RexEngine
    {
        public RexEngine()
            : this(RegexOptions.None, Encoding.ASCII)
        {
        }

        public RexEngine(int randomSeed)
            : this(RegexOptions.None, Encoding.ASCII, randomSeed)
        {
        }

        public RexEngine(RegexOptions options)
            : this(options, Encoding.ASCII)
        {
        }

        public RexEngine(Encoding encoding)
            : this(RegexOptions.None, encoding)
        {
        }

        public RexEngine(RegexOptions options, int randomSeed)
            : this(options, Encoding.ASCII, randomSeed)
        {
        }

        public RexEngine(Encoding encoding, int randomSeed)
            : this(RegexOptions.None, encoding, randomSeed)
        {
        }

        public RexEngine(RegexOptions options, Encoding encoding)
        {
            if (!options.IsValid())
                throw new ArgumentException($"The { nameof(RegexOptions) } provided must be a valid enum.", nameof(options));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            _options = options;

            var nbits = GetEncodingBitSize(encoding);
            _solver = new BddBuilder(nbits);
            _chooser = new Chooser();

            var categoryProvider = new UnicodeCategoryConditionsBddProvider(encoding, nbits);
            _regexConverter = new RegexToSFA<BinaryDecisionDiagram>(_solver, categoryProvider);
        }

        public RexEngine(RegexOptions options, Encoding encoding, int randomSeed)
        {
            if (!options.IsValid())
                throw new ArgumentException($"The { nameof(RegexOptions) } provided must be a valid enum.", nameof(options));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            _options = options;

            var nbits = GetEncodingBitSize(encoding);
            _solver = new BddBuilder(nbits);
            _chooser = new Chooser(randomSeed);

            var categoryProvider = new UnicodeCategoryConditionsBddProvider(encoding, nbits);
            _regexConverter = new RegexToSFA<BinaryDecisionDiagram>(_solver, categoryProvider);
        }

        public IEnumerable<string> GenerateMembers(string regex)
        {
            if (string.IsNullOrWhiteSpace(regex))
                throw new ArgumentNullException(nameof(regex));

            var sfa = CreateSFAFromRegex(regex);
            return GenerateMembers(sfa);
        }

        public IEnumerable<string> GenerateMembers(string regex, int count)
        {
            if (string.IsNullOrWhiteSpace(regex))
                throw new ArgumentNullException(nameof(regex));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "The number of values to generate must be non-negative.");

            var sfa = CreateSFAFromRegex(regex);
            return GenerateMembers(sfa, count);
        }

        private IEnumerable<string> GenerateMembers(SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa)
        {
            if (sfa == null)
                throw new ArgumentNullException(nameof(sfa));
            if (sfa.IsEmpty)
                throw new ArgumentException("Cannot generate a member for an empty state machine.", nameof(sfa));

            var generatedValues = new HashSet<string>();
            while (true)
            {
                var member = GenerateMember(sfa);
                var tryCount = Math.Min(100 + generatedValues.Count, 200);
                while (generatedValues.Contains(member) && tryCount-- > 0)
                    member = GenerateMember(sfa);
                if (tryCount < 0 && generatedValues.Contains(member))
                    break;
                generatedValues.Add(member);
                yield return member;
            }
        }

        private IEnumerable<string> GenerateMembers(SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa, int count)
        {
            if (sfa == null)
                throw new ArgumentNullException(nameof(sfa));
            if (sfa.IsEmpty)
                throw new ArgumentException("Cannot generate a member for an empty state machine.", nameof(sfa));

            var generatedValues = new HashSet<string>();
            for (var i = 0; i < count; ++i)
            {
                var member = GenerateMember(sfa);
                var tryCount = Math.Min(100 + generatedValues.Count, 200);
                while (generatedValues.Contains(member) && tryCount-- > 0)
                    member = GenerateMember(sfa);
                if (tryCount < 0 && generatedValues.Contains(member))
                    break;
                generatedValues.Add(member);
                yield return member;
            }
        }

        private SymbolicFiniteAutomaton<BinaryDecisionDiagram> CreateSFAFromRegex(string regex) => _regexConverter.Convert(regex, _options);

        /* Uncomment if we want combinations of regexes together, probably not though
        private SymbolicFiniteAutomaton<BinaryDecisionDiagram> CreateSFAFromRegexes(params string[] regexes)
        {
            SymbolicFiniteAutomaton<BinaryDecisionDiagram> result = null;
            foreach (var regex in regexes)
            {
                var sfa = _regexConverter.Convert(regex, _options);
                result = result == null ? sfa : SymbolicFiniteAutomaton<BinaryDecisionDiagram>.MkProduct(result, sfa, _solver.And, _solver.Or, b => b != _solver.False);
                if (result.IsEmpty)
                    break;
            }
            return result;
        }
        */

        private string GenerateMember(SymbolicFiniteAutomaton<BinaryDecisionDiagram> fa)
        {
            var builder = new StringBuilder();
            Move<BinaryDecisionDiagram> nthMoveFrom;
            for (var state = fa.InitialState; !fa.IsFinalState(state) || (fa.OutDegree(state) > 0 && _chooser.ChooseBoolean()); state = nthMoveFrom.TargetState)
            {
                nthMoveFrom = fa.GetNthMoveFrom(state, _chooser.Choose(fa.GetMovesCountFrom(state)));
                if (!nthMoveFrom.IsEpsilon)
                    builder.Append(_solver.GenerateMember(_chooser, nthMoveFrom.Condition));
            }
            return builder.ToString();
        }

        private static int GetEncodingBitSize(Encoding encoding)
        {
            if (encoding == Encoding.ASCII)
                return 7;

            return encoding.IsSingleByte
                ? 8
                : 16;
        }

        private readonly RegexOptions _options;
        private readonly Chooser _chooser;
        private readonly BddBuilder _solver;
        private readonly RegexToSFA<BinaryDecisionDiagram> _regexConverter;

        private const int TryLimitMin = 100;
        private const int TryLimitMax = 200;
    }
}
