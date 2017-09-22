using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EnumsNET;
using SJP.GenerationRex.RegularExpressions;

namespace SJP.GenerationRex
{
    internal class RegexToSFA<TConstraint>
    {
        private const int SETLENGTH = 1;
        private const int CATEGORYLENGTH = 2;
        private const int SETSTART = 3;
        private const char Lastchar = '\xFFFF';
        private readonly ICharacterConstraintSolver<TConstraint> _solver;
        private readonly IUnicodeCategoryConditions<TConstraint> _categorizer;

        public RegexToSFA(ICharacterConstraintSolver<TConstraint> solver, IUnicodeCategoryConditions<TConstraint> categorizer)
        {
            _solver = solver;
            _categorizer = categorizer;
        }

        public SymbolicFiniteAutomaton<TConstraint> Convert(string regex, RegexOptions options)
        {
            RegexOptions op = options.RemoveFlags(RegexOptions.RightToLeft);
            return ConvertNode(RegexParser.Parse(regex, op).Root, 0, true, true);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNode(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            switch (node._type)
            {
                case RegexNode.Oneloop:
                    return ConvertNodeOneloop(node, minStateId, isStart, isEnd);
                case RegexNode.Notoneloop:
                    return ConvertNodeNotoneloop(node, minStateId, isStart, isEnd);
                case RegexNode.Setloop:
                    return ConvertNodeSetloop(node, minStateId, isStart, isEnd);
                case RegexNode.Onelazy:
                case RegexNode.Notonelazy:
                case RegexNode.Setlazy:
                    throw new RexException(RexException.NotSupported);
                case RegexNode.One:
                    return ConvertNodeOne(node, minStateId, isStart, isEnd);
                case RegexNode.Notone:
                    return ConvertNodeNotone(node, minStateId, isStart, isEnd);
                case RegexNode.Set:
                    return ConvertNodeSet(node, minStateId, isStart, isEnd);
                case RegexNode.Multi:
                    return ConvertNodeMulti(node, minStateId, isStart, isEnd);
                case RegexNode.Ref:
                    throw new RexException(RexException.NotSupported);
                case RegexNode.Bol:
                    return ConvertNodeBol(node, minStateId, isStart, isEnd);
                case RegexNode.Eol:
                    return ConvertNodeEol(node, minStateId, isStart, isEnd);
                case RegexNode.Boundary:
                case RegexNode.Nonboundary:
                    throw new RexException(RexException.NotSupported);
                case RegexNode.Beginning:
                    return ConvertNodeBeginning(node, minStateId, isStart, isEnd);
                case RegexNode.Start:
                    throw new RexException(RexException.NotSupported);
                case RegexNode.EndZ:
                    return ConvertNodeEndZ(node, minStateId, isStart, isEnd);
                case RegexNode.End:
                    return ConvertNodeEnd(node, minStateId, isStart, isEnd);
                case RegexNode.Nothing:
                    throw new RexException(RexException.NotSupported);
                case RegexNode.Empty:
                    return ConvertNodeEmpty(node, minStateId, isStart, isEnd);
                case RegexNode.Alternate:
                    return ConvertNodeAlternate(node, minStateId, isStart, isEnd);
                case RegexNode.Concatenate:
                    return ConvertNodeConcatenate(node, minStateId, isStart, isEnd);
                case RegexNode.Loop:
                    return ConvertNodeLoop(node, minStateId, isStart, isEnd);
                case RegexNode.Lazyloop:
                    throw new RexException(RexException.NotSupported);
                case RegexNode.Capture:
                    return ConvertNode(node.Child(0), minStateId, isStart, isEnd);
                case RegexNode.Group:
                case RegexNode.Require:
                case RegexNode.Prevent:
                case RegexNode.Greedy:
                case RegexNode.Testref:
                case RegexNode.Testgroup:
                case RegexNode.ECMABoundary:
                case RegexNode.NonECMABoundary:
                    throw new RexException(RexException.NotSupported);
                default:
                    throw new RexException(RexException.UnrecognizedRegex);
            }
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEmpty(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            return !isStart && !isEnd
                ? SymbolicFiniteAutomaton<TConstraint>.Epsilon
                : SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, _solver.True) });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeMulti(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            int length = str.Length;
            bool caseInsensitive = node._options.HasAnyFlags(RegexOptions.IgnoreCase);
            int sourceStateId = minStateId;
            var moveList = new List<Move<TConstraint>>();
            for (int i = 0; i < length;i++)
            {
                char c = str[i];
                var charRanges = new List<char[]> { new[] { c, c } };
                var index2 = _solver.CreateRangedConstraint(caseInsensitive, charRanges);
                moveList.Add(Move<TConstraint>.To(sourceStateId + i, sourceStateId + i + 1, index2));
            }
            int finalState = sourceStateId + length;
            var finalStates = new[] { finalState };
            var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(sourceStateId, finalStates, moveList);
            sfa._isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(sourceStateId, sourceStateId, _solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(finalState, finalState, _solver.True));
            sfa._isEpsilonFree = true;
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNotone(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var index = _solver.Not(_solver.CreateCharConstraint(node._options.HasAnyFlags(RegexOptions.IgnoreCase), node._ch));
            var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId + 1 }, new[] { Move<TConstraint>.To(minStateId, minStateId + 1, index) });
            sfa._isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(minStateId, minStateId, _solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(minStateId + 1, minStateId + 1, _solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeOne(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var index = _solver.CreateCharConstraint(node._options.HasAnyFlags(RegexOptions.IgnoreCase), node._ch);
            var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId + 1 }, new[] { Move<TConstraint>.To(minStateId, minStateId + 1, index) });
            sfa._isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(minStateId, minStateId, _solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(minStateId + 1, minStateId + 1, _solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeSet(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            var conditionFromSet = CreateConditionFromSet(node._options.HasAnyFlags(RegexOptions.IgnoreCase), str);
            if (conditionFromSet.Equals(_solver.False))
                return SymbolicFiniteAutomaton<TConstraint>.Empty;
            int num = minStateId + 1;
            var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { num }, new[] { Move<TConstraint>.To(minStateId, num, conditionFromSet) });
            sfa._isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(minStateId, minStateId, _solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(num, num, _solver.True));
            sfa._isEpsilonFree = true;
            return sfa;
        }

        private TConstraint CreateConditionFromSet(bool ignoreCase, string set)
        {
            bool isNegated = RegexCharClass.IsNegated(set);
            var stateList = new List<TConstraint>();
            foreach (var range in ComputeRanges(set))
            {
                var constraint = _solver.CreateRangeConstraint(ignoreCase, range.First, range.Second);
                stateList.Add(isNegated ? _solver.Not(constraint) : constraint);
            }
            int num1 = set[1];
            int num2 = set[2];
            int num3 = num1 + 3;
            int startIndex = num3;
            while (startIndex < num3 + num2)
            {
                var codePoint = (short)set[startIndex++];
                if (codePoint != 0)
                {
                    var catNum = Math.Abs(codePoint) - 1;
                    var condition = catNum == 99
                        ? _categorizer.WhiteSpaceCondition
                        : MapCategoryCodeToCondition((UnicodeCategory)catNum);
                    stateList.Add(codePoint < 0 ^ isNegated ? _solver.Not(condition) : condition);
                }
                else
                {
                    var secondCodePoint = (short)set[startIndex++];
                    if (secondCodePoint != 0)
                    {
                        var catCodes = new HashSet<UnicodeCategory>();
                        bool isInvalidCodePoint = secondCodePoint < 0;
                        for (; secondCodePoint != 0; secondCodePoint = (short)set[startIndex++])
                        {
                            var cat = Math.Abs(secondCodePoint) - 1;
                            catCodes.Add((UnicodeCategory)cat);
                        }
                        var condition = MapCategoryCodeSetToCondition(catCodes);
                        var s = isNegated ^ isInvalidCodePoint ? _solver.Not(condition) : condition;
                        stateList.Add(s);
                    }
                }
            }
            var constraint1 = default(TConstraint);
            if (set.Length > startIndex)
            {
                string set1 = set.Substring(startIndex);
                constraint1 = CreateConditionFromSet(ignoreCase, set1);
            }
            var result = stateList.Count != 0
                ? (isNegated ? _solver.And(stateList) : _solver.Or(stateList))
                : (isNegated ? _solver.False : _solver.True);
            if (constraint1 != null)
                result = _solver.And(result, _solver.Not(constraint1));
            return result;
        }

        private static IEnumerable<Pair<char, char>> ComputeRanges(string set)
        {
            int capacity = set[1];
            var pairList = new List<Pair<char, char>>(capacity);
            int index1 = 3;
            int num = index1 + capacity;
            while (index1 < num)
            {
                char first = set[index1];
                int index2 = index1 + 1;
                char second = index2 >= num ? char.MaxValue : (char)(set[index2] - 1U);
                index1 = index2 + 1;
                pairList.Add(new Pair<char, char>(first, second));
            }
            return pairList;
        }

        private TConstraint MapCategoryCodeSetToCondition(ISet<UnicodeCategory> catCodes)
        {
            var result = default(TConstraint);

            var wordLetterCategories = new[]
            {
                UnicodeCategory.UppercaseLetter,
                UnicodeCategory.LowercaseLetter,
                UnicodeCategory.TitlecaseLetter,
                UnicodeCategory.ModifierLetter,
                UnicodeCategory.OtherLetter,
                UnicodeCategory.DecimalDigitNumber,
                UnicodeCategory.ConnectorPunctuation
            };

            var containsAllWordLetterCats = wordLetterCategories.All(catCodes.Contains);
            if (containsAllWordLetterCats)
            {
                foreach (var wlCat in wordLetterCategories)
                    catCodes.Remove(wlCat);

                result = _categorizer.WordLetterCondition;
            }

            foreach (var catCode in catCodes)
            {
                var condition = MapCategoryCodeToCondition(catCode);
                result = ReferenceEquals(result, null) ? condition : _solver.Or(result, condition);
            }
            return result;
        }

        private TConstraint MapCategoryCodeToCondition(UnicodeCategory code)
        {
            if (!code.IsValid())
                throw new ArgumentException($"The { nameof(UnicodeCategory) } provided must be a valid enum.", nameof(code));

            return _categorizer.CategoryCondition(code);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEnd(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException(RexException.MisplacedEndAnchor);
            if (!isStart)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            return SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, _solver.True) });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEndZ(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException(RexException.MisplacedEndAnchor);
            if (!isStart)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            return SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, _solver.True) });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeBeginning(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException(RexException.MisplacedStartAnchor);
            if (!isEnd)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            return SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, _solver.True) });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeBol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException(RexException.MisplacedStartAnchor);
            var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId + 2 }, new[]
            {
                Move<TConstraint>.Epsilon(minStateId, minStateId + 2),
                Move<TConstraint>.Epsilon(minStateId, minStateId + 1),
                Move<TConstraint>.To(minStateId + 1, minStateId + 1, _solver.True),
                Move<TConstraint>.To(minStateId + 1, minStateId + 2, _solver.CreateCharConstraint(false, '\n'))
            });
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(sfa.FinalState, sfa.FinalState, _solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException(RexException.MisplacedEndAnchor);
            var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId + 2 }, new[]
            {
                Move<TConstraint>.Epsilon(minStateId, minStateId + 2),
                Move<TConstraint>.Epsilon(minStateId + 1, minStateId + 2),
                Move<TConstraint>.To(minStateId + 1, minStateId + 1, _solver.True),
                Move<TConstraint>.To(minStateId, minStateId + 1, _solver.CreateCharConstraint(false, '\n'))
            });
            if (isStart)
                sfa.AddMove(Move<TConstraint>.To(sfa.InitialState, sfa.InitialState, _solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeAlternate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
            int minStateId1 = minStateId + 1;
            bool addEmptyWord = false;
            foreach (var child in node._children)
            {
                var sfa = ConvertNode(child, minStateId1, isStart, isEnd);
                if (sfa == SymbolicFiniteAutomaton<TConstraint>.Empty)
                    continue;

                if (sfa == SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                {
                    addEmptyWord = true;
                }
                else
                {
                    if (sfa.IsFinalState(sfa.InitialState))
                        addEmptyWord = true;
                    sfas.Add(sfa);
                    minStateId1 = sfa.MaxState + 1;
                }
            }
            return AlternateSFAs(minStateId, sfas, addEmptyWord);
        }

        private SymbolicFiniteAutomaton<TConstraint> AlternateSFAs(int start, List<SymbolicFiniteAutomaton<TConstraint>> sfas, bool addEmptyWord)
        {
            if (sfas.Count == 0)
            {
                if (addEmptyWord)
                    return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
                return SymbolicFiniteAutomaton<TConstraint>.Empty;
            }
            if (sfas.Count == 1)
            {
                if (addEmptyWord && !sfas[0].IsFinalState(sfas[0].InitialState))
                {
                    if (sfas[0].InitialStateIsSource)
                        sfas[0].MakeInitialStateFinal();
                    else
                        sfas[0].AddNewInitialStateThatIsFinal(start);
                }
                return sfas[0];
            }
            bool flag1 = true;
            foreach (SymbolicFiniteAutomaton<TConstraint> sfa in sfas)
            {
                if (!sfa.InitialStateIsSource)
                {
                    flag1 = false;
                    break;
                }
            }
            bool flag2 = !sfas.Exists(IsNonDeterministic);
            sfas.Exists(HasEpsilons);
            bool flag3 = true;
            int val2 = int.MinValue;
            foreach (SymbolicFiniteAutomaton<TConstraint> sfa in sfas)
            {
                if (!sfa.HasSingleFinalSink)
                {
                    flag3 = false;
                    break;
                }
                val2 = Math.Max(sfa.FinalState, val2);
            }
            var intList = new List<int>();
            if (addEmptyWord)
                intList.Add(start);
            var condMap = new Dictionary<Pair<int, int>, TConstraint>();
            var pairSet = new HashSet<Pair<int, int>>();
            if (!flag1)
            {
                flag2 = false;
                foreach (SymbolicFiniteAutomaton<TConstraint> sfa in sfas)
                    pairSet.Add(new Pair<int, int>(start, sfa.InitialState));
            }
            else if (flag2)
            {
                for (int index1 = 0; index1 < sfas.Count - 1; ++index1)
                {
                    for (int index2 = index1 + 1; index2 < sfas.Count; ++index2)
                    {
                        TConstraint constraint1 = _solver.False;
                        foreach (Move<TConstraint> move in sfas[index1].GetMovesFrom(sfas[index1].InitialState))
                            constraint1 = _solver.Or(constraint1, move.Condition);
                        TConstraint s = _solver.False;
                        foreach (Move<TConstraint> move in sfas[index2].GetMovesFrom(sfas[index2].InitialState))
                            s = _solver.Or(s, move.Condition);
                        flag2 = _solver.And(constraint1, s).Equals(_solver.False);
                        if (!flag2)
                            break;
                    }
                    if (!flag2)
                        break;
                }
            }
            if (flag3)
                intList.Add(val2);
            var dictionary = new Dictionary<int, int>();
            foreach (var sfa in sfas)
            {
                foreach (var move in sfa.GetMoves())
                {
                    int first = !flag1 || sfa.InitialState != move.SourceState ? move.SourceState : start;
                    int second = !flag3 || sfa.FinalState != move.TargetState ? move.TargetState : val2;
                    var key = new Pair<int, int>(first, second);
                    dictionary[move.SourceState] = first;
                    dictionary[move.TargetState] = second;
                    if (move.IsEpsilon)
                    {
                        if (first != second)
                            pairSet.Add(new Pair<int, int>(first, second));
                    }
                    else
                    {
                        condMap[key] = !condMap.TryGetValue(key, out var constraint1) ? move.Condition : _solver.Or(constraint1, move.Condition);
                    }
                }
                if (!flag3)
                {
                    foreach (int finalState in sfa.GetFinalStates())
                    {
                        int num = dictionary[finalState];
                        if (!intList.Contains(num))
                            intList.Add(num);
                    }
                }
            }
            var resultSfa = SymbolicFiniteAutomaton<TConstraint>.Create(start, intList, GenerateMoves(condMap, pairSet));
            resultSfa._isDeterministic = flag2;
            return resultSfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeConcatenate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var children = node._children;
            var sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
            int minStateId1 = minStateId;
            for (int index = 0; index < children.Count; ++index)
            {
                var sfa = ConvertNode(children[index], minStateId1, isStart && index == 0, isEnd && index == children.Count - 1);
                if (sfa == SymbolicFiniteAutomaton<TConstraint>.Empty)
                    return SymbolicFiniteAutomaton<TConstraint>.Empty;
                if (sfa == SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                    continue;

                sfas.Add(sfa);
                minStateId1 = sfa.MaxState + 1;
            }
            return ConcatenateSFAs(sfas);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConcatenateSFAs(List<SymbolicFiniteAutomaton<TConstraint>> sfas)
        {
            if (sfas.Count == 0)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            if (sfas.Count == 1)
                return sfas[0];
            var sfa = sfas[0];
            for (int index = 1; index < sfas.Count; ++index)
                sfa.Concat(sfas[index]);
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeLoop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var sfa = ConvertNode(node._children[0], minStateId, false, false);
            int m = node._m;
            int n = node._n;
            SymbolicFiniteAutomaton<TConstraint> loop;
            if (m == 0 && sfa.IsEmpty)
            {
                loop = SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            }
            else if (m == 0 && n == int.MaxValue)
            {
                loop = MakeKleeneClosure(sfa);
            }
            else if (m == 0 && n == 1)
            {
                if (sfa.IsFinalState(sfa.InitialState))
                    return sfa;
                if (sfa.InitialStateIsSource)
                    sfa.MakeInitialStateFinal();
                else
                    sfa.AddNewInitialStateThatIsFinal(sfa.MaxState + 1);
                loop = sfa;
            }
            else if (m == 1 && n == 1)
            {
                if (sfa.IsEmpty)
                    return SymbolicFiniteAutomaton<TConstraint>.Empty;
                loop = sfa;
            }
            else if (n == int.MaxValue)
            {
                if (sfa.IsEmpty)
                    return SymbolicFiniteAutomaton<TConstraint>.Empty;
                if (sfa.IsFinalState(sfa.InitialState))
                {
                    loop = MakeKleeneClosure(sfa);
                }
                else
                {
                    var sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
                    for (int index = 0; index < m; ++index)
                    {
                        sfas.Add(sfa);
                        sfa = sfa.MakeCopy(sfa.MaxState + 1);
                    }
                    sfas.Add(MakeKleeneClosure(sfa));
                    loop = ConcatenateSFAs(sfas);
                }
            }
            else
            {
                var sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
                for (int index = 0; index < n; ++index)
                {
                    sfas.Add(sfa);
                    if (index < n - 1)
                    {
                        sfa = sfa.MakeCopy(sfa.MaxState + 1);
                        if (index >= m - 1)
                        {
                            if (sfa.InitialStateIsSource && !sfa.IsFinalState(sfa.InitialState))
                                sfa.MakeInitialStateFinal();
                            else
                                sfa.AddNewInitialStateThatIsFinal(sfa.MaxState + 1);
                        }
                    }
                }
                loop = ConcatenateSFAs(sfas);
            }
            return ExtendLoop(minStateId, isStart, isEnd, loop);
        }

        private SymbolicFiniteAutomaton<TConstraint> ExtendLoop(int minStateId, bool isStart, bool isEnd, SymbolicFiniteAutomaton<TConstraint> loop)
        {
            if (isStart)
            {
                if (loop != SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                {
                    var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(loop.MaxState + 1, new[] { loop.MaxState + 1 }, new[] { Move<TConstraint>.To(loop.MaxState + 1, loop.MaxState + 1, _solver.True) });
                    sfa.Concat(loop);
                    loop = sfa;
                }
                else
                {
                    loop = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, _solver.True) });
                }
            }
            if (isEnd)
            {
                if (loop != SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                {
                    loop.Concat(SymbolicFiniteAutomaton<TConstraint>.Create(loop.MaxState + 1, new[] { loop.MaxState + 1 }, new[] { Move<TConstraint>.To(loop.MaxState + 1, loop.MaxState + 1, _solver.True) }));
                }
                else
                {
                    loop = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, _solver.True) });
                }
            }
            return loop;
        }

        private SymbolicFiniteAutomaton<TConstraint> MakeKleeneClosure(SymbolicFiniteAutomaton<TConstraint> sfa)
        {
            if (sfa == SymbolicFiniteAutomaton<TConstraint>.Empty || sfa == SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            if (sfa.IsKleeneClosure())
                return sfa;
            if (sfa.InitialStateIsSource && sfa.HasSingleFinalSink)
            {
                sfa.RenameInitialState(sfa.FinalState);
                return sfa;
            }
            int initialState = sfa.InitialState;
            if (!sfa.IsFinalState(sfa.InitialState))
            {
                if (sfa.InitialStateIsSource)
                    sfa.MakeInitialStateFinal();
                else
                    sfa.AddNewInitialStateThatIsFinal(sfa.MaxState + 1);
            }
            foreach (int finalState in sfa.GetFinalStates())
            {
                if (finalState != sfa.InitialState && finalState != initialState)
                    sfa.AddMove(Move<TConstraint>.Epsilon(finalState, initialState));
            }
            return sfa.RemoveEpsilonLoops(new Func<TConstraint, TConstraint, TConstraint>(_solver.Or));
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNotoneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var index = _solver.Not(_solver.CreateCharConstraint(node._options.HasAnyFlags(RegexOptions.IgnoreCase), node._ch));
            var loopFromCondition = CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeOneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var index = _solver.CreateCharConstraint(node._options.HasAnyFlags(RegexOptions.IgnoreCase), node._ch);
            var loopFromCondition = CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeSetloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var str = node._str;
            var conditionFromSet = CreateConditionFromSet(node._options.HasAnyFlags(RegexOptions.IgnoreCase), str);
            if (conditionFromSet.Equals(_solver.False))
            {
                return node._m == 0
                    ? SymbolicFiniteAutomaton<TConstraint>.Epsilon
                    : SymbolicFiniteAutomaton<TConstraint>.Empty;
            }
            var loopFromCondition = CreateLoopFromCondition(minStateId, conditionFromSet, node._m, node._n);
            return ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private static SymbolicFiniteAutomaton<TConstraint> CreateLoopFromCondition(int minStateId, TConstraint cond, int m, int n)
        {
            if (m == 0 && n == int.MaxValue)
            {
                var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, cond) });
                sfa._isEpsilonFree = true;
                sfa._isDeterministic = true;
                return sfa;
            }
            if (m == 0 && n == 1)
            {
                var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId, minStateId + 1 }, new[] { Move<TConstraint>.To(minStateId, minStateId + 1, cond) });
                sfa._isEpsilonFree = true;
                sfa._isDeterministic = true;
                return sfa;
            }
            if (n == int.MaxValue)
            {
                var moveList = new List<Move<TConstraint>>();
                for (int index = 0; index < m; ++index)
                    moveList.Add(Move<TConstraint>.To(minStateId + index, minStateId + index + 1, cond));
                moveList.Add(Move<TConstraint>.To(minStateId + m, minStateId + m, cond));
                var sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId + m }, moveList);
                sfa._isDeterministic = true;
                sfa._isEpsilonFree = true;
                return sfa;
            }
            var moveArray = new List<Move<TConstraint>>();
            for (int index = 0; index < n; ++index)
                moveArray.Add(Move<TConstraint>.To(minStateId + index, minStateId + index + 1, cond));
            var numArray = new int[n + 1 - m];
            for (int index = m; index <= n; ++index)
                numArray[index - m] = index + minStateId;
            var resultSfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, numArray, moveArray);
            resultSfa._isEpsilonFree = true;
            resultSfa._isDeterministic = true;
            return resultSfa;
        }

        private static bool IsNonDeterministic(SymbolicFiniteAutomaton<TConstraint> sfa) => !sfa._isDeterministic;

        private static bool HasEpsilons(SymbolicFiniteAutomaton<TConstraint> sfa) => !sfa._isEpsilonFree;

        private IEnumerable<Move<TConstraint>> GenerateMoves(Dictionary<Pair<int, int>, TConstraint> condMap, IEnumerable<Pair<int, int>> epsilonMoves)
        {
            var moves = condMap.Select(condition => Move<TConstraint>.To(condition.Key.First, condition.Key.Second, condition.Value));
            var eMoves = epsilonMoves.Select(eMove => Move<TConstraint>.Epsilon(eMove.First, eMove.Second));
            return moves.Concat(eMoves);
        }
    }
}
