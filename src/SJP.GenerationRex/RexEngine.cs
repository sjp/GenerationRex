using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EnumsNET;

namespace SJP.GenerationRex;

/// <summary>
/// An engine used to generate members of a regular expression.
/// </summary>
public class RexEngine
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class.
    /// </summary>
    /// <remarks>The resulting object will generate members using no regular expression options (i.e. <see cref="RegexOptions.None"/>), along with generating from the <see cref="Encoding.ASCII"/> character set.</remarks>
    public RexEngine()
        : this(RegexOptions.None, Encoding.ASCII)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class, using a specified seed value.
    /// </summary>
    /// <param name="randomSeed">A number used to ensure deterministic member generation. If a negative number is specified, the absolute value of the number is used.</param>
    /// <remarks>The resulting object will generate members using no regular expression options (i.e. <see cref="RegexOptions.None"/>), along with generating from the <see cref="Encoding.ASCII"/> character set.</remarks>
    public RexEngine(int randomSeed)
        : this(RegexOptions.None, Encoding.ASCII, randomSeed)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class.
    /// </summary>
    /// <param name="options">A bitwise combination of the <see cref="RegexOptions"/> values that provide options for generation members from a regular expression.</param>
    /// <remarks>The resulting object will generate members from the <see cref="Encoding.ASCII"/> character set.</remarks>
    /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid enum.</exception>
    public RexEngine(RegexOptions options)
        : this(options, Encoding.ASCII)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class.
    /// </summary>
    /// <param name="encoding">An encoding, representing a character set to generate members from.</param>
    /// <remarks>The resulting object will generate members using no regular expression options (i.e. <see cref="RegexOptions.None"/>).</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
    public RexEngine(Encoding encoding)
        : this(RegexOptions.None, encoding)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class, using a specified seed value.
    /// </summary>
    /// <param name="options">A bitwise combination of the <see cref="RegexOptions"/> values that provide options for generation members from a regular expression.</param>
    /// <param name="randomSeed">A number used to ensure deterministic member generation. If a negative number is specified, the absolute value of the number is used.</param>
    /// <remarks>The resulting object will generate members from the <see cref="Encoding.ASCII"/> character set.</remarks>
    /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid enum.</exception>
    public RexEngine(RegexOptions options, int randomSeed)
        : this(options, Encoding.ASCII, randomSeed)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class, using a specified seed value.
    /// </summary>
    /// <param name="encoding">An encoding, representing a character set to generate members from.</param>
    /// <param name="randomSeed">A number used to ensure deterministic member generation. If a negative number is specified, the absolute value of the number is used.</param>
    /// <remarks>The resulting object will generate members using no regular expression options (i.e. <see cref="RegexOptions.None"/>).</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
    public RexEngine(Encoding encoding, int randomSeed)
        : this(RegexOptions.None, encoding, randomSeed)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class.
    /// </summary>
    /// <param name="options">A bitwise combination of the <see cref="RegexOptions"/> values that provide options for generation members from a regular expression.</param>
    /// <param name="encoding">An encoding, representing a character set to generate members from.</param>
    /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid enum.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
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
        _regexConverter = new RegexToSfa<BinaryDecisionDiagram>(_solver, categoryProvider);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RexEngine"/> class, using a specified seed value.
    /// </summary>
    /// <param name="options">A bitwise combination of the <see cref="RegexOptions"/> values that provide options for generation members from a regular expression.</param>
    /// <param name="encoding">An encoding, representing a character set to generate members from.</param>
    /// <param name="randomSeed">A number used to ensure deterministic member generation. If a negative number is specified, the absolute value of the number is used.</param>
    /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid enum.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
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
        _regexConverter = new RegexToSfa<BinaryDecisionDiagram>(_solver, categoryProvider);
    }

    /// <summary>
    /// Generates a collection of values from the set defined by a regular expression.
    /// </summary>
    /// <param name="regex">A regular expression.</param>
    /// <returns>A collection of members of a regular expression.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="regex"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="regex"/> is not a valid regular expression.</exception>
    /// <exception cref="RexException"><paramref name="regex"/> is not a valid regular expression to generate from.</exception>
    /// <remarks>The resulting set of members is unique, and because it is generated randomly it may terminate before generating all values from the set defined by the regular expression.</remarks>
    public IEnumerable<string> GenerateMembers(string regex)
    {
        if (string.IsNullOrWhiteSpace(regex))
            throw new ArgumentNullException(nameof(regex));

        var sfa = CreateSFAFromRegex(regex);
        return GenerateMembers(sfa);
    }

    /// <summary>
    /// Generates a collection of values from the set defined by a regular expression.
    /// </summary>
    /// <param name="regex">A regular expression.</param>
    /// <param name="count">The maximum amount of members to generate.</param>
    /// <returns>A collection of members of a regular expression.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="regex"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is a negative number.</exception>
    /// <exception cref="ArgumentException"><paramref name="regex"/> is not a valid regular expression.</exception>
    /// <exception cref="RexException"><paramref name="regex"/> is not a valid regular expression to generate from.</exception>
    /// <remarks>The resulting set of members is unique, and because it is generated randomly it may terminate before generating all values from the set defined by the regular expression. Consequently, it may terminate before the maximum count of values <paramref name="count"/>, has been reached.</remarks>
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

        return GenerateMembersIterator();
        IEnumerable<string> GenerateMembersIterator()
        {
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
    }

    private IEnumerable<string> GenerateMembers(SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa, int count)
    {
        if (sfa == null)
            throw new ArgumentNullException(nameof(sfa));
        if (sfa.IsEmpty)
            throw new ArgumentException("Cannot generate a member for an empty state machine.", nameof(sfa));

        return GenerateMembersIterator();
        IEnumerable<string> GenerateMembersIterator()
        {
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
    private readonly RegexToSfa<BinaryDecisionDiagram> _regexConverter;
}