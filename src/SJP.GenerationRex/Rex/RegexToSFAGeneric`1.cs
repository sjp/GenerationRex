using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Rex
{
    internal class RegexToSFAGeneric<S>
    {
        private Dictionary<S, string> description = new Dictionary<S, string>();
        private const int SETLENGTH = 1;
        private const int CATEGORYLENGTH = 2;
        private const int SETSTART = 3;
        private const char Lastchar = '\xFFFF';
        private ICharacterConstraintSolver<S> solver;
        private IUnicodeCategoryConditions<S> categorizer;

        public RegexToSFAGeneric(ICharacterConstraintSolver<S> solver, IUnicodeCategoryConditions<S> categorizer)
        {
            this.solver = solver;
            this.categorizer = categorizer;
            description.Add(solver.True, "");
        }

        public SymbolicFiniteAutomaton<S> Convert(string regex, RegexOptions options)
        {
            RegexOptions op = options & ~RegexOptions.RightToLeft;
            return ConvertNode(RegexParser.Parse(regex, op)._root, 0, true, true);
        }

        private SymbolicFiniteAutomaton<S> ConvertNode(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            switch (node._type)
            {
                case 3:
                    return ConvertNodeOneloop(node, minStateId, isStart, isEnd);
                case 4:
                    return ConvertNodeNotoneloop(node, minStateId, isStart, isEnd);
                case 5:
                    return ConvertNodeSetloop(node, minStateId, isStart, isEnd);
                case 6:
                    return ConvertNodeOnelazy(node, minStateId, isStart, isEnd);
                case 7:
                    return ConvertNodeNotonelazy(node, minStateId, isStart, isEnd);
                case 8:
                    return ConvertNodeSetlazy(node, minStateId, isStart, isEnd);
                case 9:
                    return ConvertNodeOne(node, minStateId, isStart, isEnd);
                case 10:
                    return ConvertNodeNotone(node, minStateId, isStart, isEnd);
                case 11:
                    return ConvertNodeSet(node, minStateId, isStart, isEnd);
                case 12:
                    return ConvertNodeMulti(node, minStateId, isStart, isEnd);
                case 13:
                    return ConvertNodeRef(node, minStateId, isStart, isEnd);
                case 14:
                    return ConvertNodeBol(node, minStateId, isStart, isEnd);
                case 15:
                    return ConvertNodeEol(node, minStateId, isStart, isEnd);
                case 16:
                    return ConvertNodeBoundary(node, minStateId, isStart, isEnd);
                case 17:
                    return ConvertNodeNonboundary(node, minStateId, isStart, isEnd);
                case 18:
                    return ConvertNodeBeginning(node, minStateId, isStart, isEnd);
                case 19:
                    return ConvertNodeStart(node, minStateId, isStart, isEnd);
                case 20:
                    return ConvertNodeEndZ(node, minStateId, isStart, isEnd);
                case 21:
                    return ConvertNodeEnd(node, minStateId, isStart, isEnd);
                case 22:
                    return ConvertNodeNothing(node, minStateId, isStart, isEnd);
                case 23:
                    return ConvertNodeEmpty(node, minStateId, isStart, isEnd);
                case 24:
                    return ConvertNodeAlternate(node, minStateId, isStart, isEnd);
                case 25:
                    return ConvertNodeConcatenate(node, minStateId, isStart, isEnd);
                case 26:
                    return ConvertNodeLoop(node, minStateId, isStart, isEnd);
                case 27:
                    return ConvertNodeLazyloop(node, minStateId, isStart, isEnd);
                case 28:
                    return ConvertNode(node.Child(0), minStateId, isStart, isEnd);
                case 29:
                    return ConvertNodeGroup(node, minStateId, isStart, isEnd);
                case 30:
                    return ConvertNodeRequire(node, minStateId, isStart, isEnd);
                case 31:
                    return ConvertNodePrevent(node, minStateId, isStart, isEnd);
                case 32:
                    return ConvertNodeGreedy(node, minStateId, isStart, isEnd);
                case 33:
                    return ConvertNodeTestref(node, minStateId, isStart, isEnd);
                case 34:
                    return ConvertNodeTestgroup(node, minStateId, isStart, isEnd);
                case 41:
                    return ConvertNodeECMABoundary(node, minStateId, isStart, isEnd);
                case 42:
                    return ConvertNodeNonECMABoundary(node, minStateId, isStart, isEnd);
                default:
                    throw new RexException("Unrecognized regex construct");
            }
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeEmpty(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart && !isEnd)
                return SymbolicFiniteAutomaton<S>.Epsilon;
            return SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId }, new Move<S>[1] { Move<S>.T(minStateId, minStateId, solver.True) });
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeMulti(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            int length = str.Length;
            bool caseInsensitive = (node._options & RegexOptions.IgnoreCase) != RegexOptions.None;
            int num1 = minStateId;
            int num2 = num1 + length;
            var numArray = new int[1] { num2 };
            var moveList = new List<Move<S>>();
            for (int index1 = 0; index1 < length; ++index1)
            {
                var chArrayList = new List<char[]>();
                char c = str[index1];
                chArrayList.Add(new char[2] { c, c });
                S index2 = solver.MkRangesConstraint(caseInsensitive, chArrayList);
                if (!description.ContainsKey(index2))
                    description[index2] = RexEngine.Escape(c);
                moveList.Add(Move<S>.T(num1 + index1, num1 + index1 + 1, index2));
            }
            SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(num1, numArray, moveList);
            sfa._isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.T(num1, num1, solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.T(num2, num2, solver.True));
            sfa._isEpsilonFree = true;
            return sfa;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeNotone(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = solver.MkNot(solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch));
            if (!description.ContainsKey(index))
                description[index] = string.Format("[^{0}]", RexEngine.Escape(node._ch));
            SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId + 1 }, new Move<S>[1] { Move<S>.T(minStateId, minStateId + 1, index) });
            sfa._isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.T(minStateId, minStateId, solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.T(minStateId + 1, minStateId + 1, solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeOne(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch);
            if (!description.ContainsKey(index))
                description[index] = RexEngine.Escape(node._ch);
            SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId + 1 }, new Move<S>[1] { Move<S>.T(minStateId, minStateId + 1, index) });
            sfa._isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.T(minStateId, minStateId, solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.T(minStateId + 1, minStateId + 1, solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeSet(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            S conditionFromSet = CreateConditionFromSet((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, str);
            if (conditionFromSet.Equals(solver.False))
                return SymbolicFiniteAutomaton<S>.Empty;
            if (!description.ContainsKey(conditionFromSet))
                description[conditionFromSet] = RegexCharClass.SetDescription(str);
            int num = minStateId + 1;
            SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { num }, new Move<S>[1] { Move<S>.T(minStateId, num, conditionFromSet) });
            sfa._isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.T(minStateId, minStateId, solver.True));
                sfa._isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.T(num, num, solver.True));
            sfa._isEpsilonFree = true;
            return sfa;
        }

        private S CreateConditionFromSet(bool ignoreCase, string set)
        {
            bool flag1 = RegexCharClass.IsNegated(set);
            var sList = new List<S>();
            foreach (Pair<char, char> range in RegexToSFAGeneric<S>.ComputeRanges(set))
            {
                S constraint = solver.MkRangeConstraint(ignoreCase, range.First, range.Second);
                sList.Add(flag1 ? solver.MkNot(constraint) : constraint);
            }
            int num1 = set[1];
            int num2 = set[2];
            int num3 = num1 + 3;
            int startIndex = num3;
            while (startIndex < num3 + num2)
            {
                var num4 = (short)set[startIndex++];
                if (num4 != 0)
                {
                    S condition = MapCategoryCodeToCondition(Math.Abs(num4) - 1);
                    sList.Add(num4 < 0 ^ flag1 ? solver.MkNot(condition) : condition);
                }
                else
                {
                    var num5 = (short)set[startIndex++];
                    if (num5 != 0)
                    {
                        var catCodes = new HashSet<int>();
                        bool flag2 = num5 < 0;
                        for (; num5 != 0; num5 = (short)set[startIndex++])
                            catCodes.Add(Math.Abs(num5) - 1);
                        S condition = MapCategoryCodeSetToCondition(catCodes);
                        S s = flag1 ^ flag2 ? solver.MkNot(condition) : condition;
                        sList.Add(s);
                    }
                }
            }
            var constraint1 = default(S);
            if (set.Length > startIndex)
            {
                string set1 = set.Substring(startIndex);
                constraint1 = CreateConditionFromSet(ignoreCase, set1);
            }
            S constraint1_1 = sList.Count != 0 ? (flag1 ? solver.MkAnd(sList) : solver.MkOr(sList)) : (flag1 ? solver.False : solver.True);
            if (constraint1 != null)
                constraint1_1 = solver.MkAnd(constraint1_1, solver.MkNot(constraint1));
            return constraint1_1;
        }

        private static List<Pair<char, char>> ComputeRanges(string set)
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

        private S MapCategoryCodeSetToCondition(HashSet<int> catCodes)
        {
            var constraint1 = default(S);
            if (catCodes.Contains(0) && catCodes.Contains(1) && (catCodes.Contains(2) && catCodes.Contains(3)) && (catCodes.Contains(4) && catCodes.Contains(8) && catCodes.Contains(18)))
            {
                catCodes.Remove(0);
                catCodes.Remove(1);
                catCodes.Remove(2);
                catCodes.Remove(3);
                catCodes.Remove(4);
                catCodes.Remove(8);
                catCodes.Remove(18);
                constraint1 = categorizer.WordLetterCondition;
            }
            foreach (int catCode in catCodes)
            {
                S condition = MapCategoryCodeToCondition(catCode);
                constraint1 = ReferenceEquals(constraint1, null) ? condition : solver.MkOr(constraint1, condition);
            }
            return constraint1;
        }

        private S MapCategoryCodeToCondition(int code)
        {
            if (code == 99)
                return categorizer.WhiteSpaceCondition;
            if (code < 0 || code > 29)
                throw new ArgumentOutOfRangeException(nameof(code), "Must be in the range 0..29 or equal to 99");
            return categorizer.CategoryCondition(code);
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeEnd(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            if (!isStart)
                return SymbolicFiniteAutomaton<S>.Epsilon;
            return SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId }, new Move<S>[1] { Move<S>.T(minStateId, minStateId, solver.True) });
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeEndZ(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            if (!isStart)
                return SymbolicFiniteAutomaton<S>.Epsilon;
            return SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId }, new Move<S>[1] { Move<S>.T(minStateId, minStateId, solver.True) });
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeBeginning(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException("The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop");
            if (!isEnd)
                return SymbolicFiniteAutomaton<S>.Epsilon;
            return SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId }, new Move<S>[1] { Move<S>.T(minStateId, minStateId, solver.True) });
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeBol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException("The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop");
            SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId + 2 }, new Move<S>[4] { Move<S>.Epsilon(minStateId, minStateId + 2), Move<S>.Epsilon(minStateId, minStateId + 1), Move<S>.T(minStateId + 1, minStateId + 1, solver.True), Move<S>.T(minStateId + 1, minStateId + 2, solver.MkCharConstraint(false, '\n')) });
            if (isEnd)
                sfa.AddMove(Move<S>.T(sfa.FinalState, sfa.FinalState, solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeEol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId + 2 }, new Move<S>[4] { Move<S>.Epsilon(minStateId, minStateId + 2), Move<S>.Epsilon(minStateId + 1, minStateId + 2), Move<S>.T(minStateId + 1, minStateId + 1, solver.True), Move<S>.T(minStateId, minStateId + 1, solver.MkCharConstraint(false, '\n')) });
            if (isStart)
                sfa.AddMove(Move<S>.T(sfa.InitialState, sfa.InitialState, solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeAlternate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            var sfas = new List<SymbolicFiniteAutomaton<S>>();
            int minStateId1 = minStateId + 1;
            bool addEmptyWord = false;
            foreach (RegexNode child in node._children)
            {
                SymbolicFiniteAutomaton<S> sfa = ConvertNode(child, minStateId1, isStart, isEnd);
                if (sfa != SymbolicFiniteAutomaton<S>.Empty)
                {
                    if (sfa == SymbolicFiniteAutomaton<S>.Epsilon)
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
            }
            return AlternateSFAs(minStateId, sfas, addEmptyWord);
        }

        private SymbolicFiniteAutomaton<S> AlternateSFAs(int start, List<SymbolicFiniteAutomaton<S>> sfas, bool addEmptyWord)
        {
            if (sfas.Count == 0)
            {
                if (addEmptyWord)
                    return SymbolicFiniteAutomaton<S>.Epsilon;
                return SymbolicFiniteAutomaton<S>.Empty;
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
            foreach (SymbolicFiniteAutomaton<S> sfa in sfas)
            {
                if (!sfa.InitialStateIsSource)
                {
                    flag1 = false;
                    break;
                }
            }
            bool flag2 = !sfas.Exists(new Predicate<SymbolicFiniteAutomaton<S>>(RegexToSFAGeneric<S>.IsNonDeterministic));
            sfas.Exists(new Predicate<SymbolicFiniteAutomaton<S>>(RegexToSFAGeneric<S>.HasEpsilons));
            bool flag3 = true;
            int val2 = int.MinValue;
            foreach (SymbolicFiniteAutomaton<S> sfa in sfas)
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
            var condMap = new Dictionary<Pair<int, int>, S>();
            var pairSet = new HashSet<Pair<int, int>>();
            if (!flag1)
            {
                flag2 = false;
                foreach (SymbolicFiniteAutomaton<S> sfa in sfas)
                    pairSet.Add(new Pair<int, int>(start, sfa.InitialState));
            }
            else if (flag2)
            {
                for (int index1 = 0; index1 < sfas.Count - 1; ++index1)
                {
                    for (int index2 = index1 + 1; index2 < sfas.Count; ++index2)
                    {
                        S constraint1 = solver.False;
                        foreach (Move<S> move in sfas[index1].GetMovesFrom(sfas[index1].InitialState))
                            constraint1 = solver.MkOr(constraint1, move.Condition);
                        S s = solver.False;
                        foreach (Move<S> move in sfas[index2].GetMovesFrom(sfas[index2].InitialState))
                            s = solver.MkOr(s, move.Condition);
                        flag2 = solver.MkAnd(constraint1, s).Equals(solver.False);
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
            foreach (SymbolicFiniteAutomaton<S> sfa in sfas)
            {
                foreach (Move<S> move in sfa.GetMoves())
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
                        S constraint1;
                        condMap[key] = !condMap.TryGetValue(key, out constraint1) ? move.Condition : solver.MkOr(constraint1, move.Condition);
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
            SymbolicFiniteAutomaton<S> sfa1 = SymbolicFiniteAutomaton<S>.Create(start, intList, GenerateMoves(condMap, pairSet));
            sfa1._isDeterministic = flag2;
            return sfa1;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeConcatenate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            List<RegexNode> children = node._children;
            var sfas = new List<SymbolicFiniteAutomaton<S>>();
            int minStateId1 = minStateId;
            for (int index = 0; index < children.Count; ++index)
            {
                SymbolicFiniteAutomaton<S> sfa = ConvertNode(children[index], minStateId1, isStart && index == 0, isEnd && index == children.Count - 1);
                if (sfa == SymbolicFiniteAutomaton<S>.Empty)
                    return SymbolicFiniteAutomaton<S>.Empty;
                if (sfa != SymbolicFiniteAutomaton<S>.Epsilon)
                {
                    sfas.Add(sfa);
                    minStateId1 = sfa.MaxState + 1;
                }
            }
            return ConcatenateSFAs(sfas);
        }

        private SymbolicFiniteAutomaton<S> ConcatenateSFAs(List<SymbolicFiniteAutomaton<S>> sfas)
        {
            if (sfas.Count == 0)
                return SymbolicFiniteAutomaton<S>.Epsilon;
            if (sfas.Count == 1)
                return sfas[0];
            SymbolicFiniteAutomaton<S> sfa = sfas[0];
            for (int index = 1; index < sfas.Count; ++index)
                sfa.Concat(sfas[index]);
            return sfa;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeLoop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            SymbolicFiniteAutomaton<S> sfa = ConvertNode(node._children[0], minStateId, false, false);
            int m = node._m;
            int n = node._n;
            SymbolicFiniteAutomaton<S> loop;
            if (m == 0 && sfa.IsEmpty)
                loop = SymbolicFiniteAutomaton<S>.Epsilon;
            else if (m == 0 && n == int.MaxValue)
                loop = MakeKleeneClosure(sfa);
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
                    return SymbolicFiniteAutomaton<S>.Empty;
                loop = sfa;
            }
            else if (n == int.MaxValue)
            {
                if (sfa.IsEmpty)
                    return SymbolicFiniteAutomaton<S>.Empty;
                if (sfa.IsFinalState(sfa.InitialState))
                {
                    loop = MakeKleeneClosure(sfa);
                }
                else
                {
                    var sfas = new List<SymbolicFiniteAutomaton<S>>();
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
                var sfas = new List<SymbolicFiniteAutomaton<S>>();
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

        private SymbolicFiniteAutomaton<S> ExtendLoop(int minStateId, bool isStart, bool isEnd, SymbolicFiniteAutomaton<S> loop)
        {
            if (isStart)
            {
                if (loop != SymbolicFiniteAutomaton<S>.Epsilon)
                {
                    SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(loop.MaxState + 1, new int[1] { loop.MaxState + 1 }, new Move<S>[1] { Move<S>.T(loop.MaxState + 1, loop.MaxState + 1, solver.True) });
                    sfa.Concat(loop);
                    loop = sfa;
                }
                else
                    loop = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1]
                    {
            minStateId
                    }, new Move<S>[1]
                    {
            Move<S>.T(minStateId, minStateId, solver.True)
                    });
            }
            if (isEnd)
            {
                if (loop != SymbolicFiniteAutomaton<S>.Epsilon)
                    loop.Concat(SymbolicFiniteAutomaton<S>.Create(loop.MaxState + 1, new int[1]
                    {
            loop.MaxState + 1
                    }, new Move<S>[1]
                    {
            Move<S>.T(loop.MaxState + 1, loop.MaxState + 1, solver.True)
                    }));
                else
                    loop = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1]
                    {
            minStateId
                    }, new Move<S>[1]
                    {
            Move<S>.T(minStateId, minStateId, solver.True)
                    });
            }
            return loop;
        }

        private SymbolicFiniteAutomaton<S> MakeKleeneClosure(SymbolicFiniteAutomaton<S> sfa)
        {
            if (sfa == SymbolicFiniteAutomaton<S>.Empty || sfa == SymbolicFiniteAutomaton<S>.Epsilon)
                return SymbolicFiniteAutomaton<S>.Epsilon;
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
                    sfa.AddMove(Move<S>.Epsilon(finalState, initialState));
            }
            return sfa.RemoveEpsilonLoops(new Func<S, S, S>(solver.MkOr));
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeNotoneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = solver.MkNot(solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch));
            if (!description.ContainsKey(index))
                description[index] = string.Format("[^{0}]", RexEngine.Escape(node._ch));
            SymbolicFiniteAutomaton<S> loopFromCondition = RegexToSFAGeneric<S>.CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeOneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch);
            if (!description.ContainsKey(index))
                description[index] = string.Format("{0}", RexEngine.Escape(node._ch));
            SymbolicFiniteAutomaton<S> loopFromCondition = RegexToSFAGeneric<S>.CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeSetloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            S conditionFromSet = CreateConditionFromSet((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, str);
            if (conditionFromSet.Equals(solver.False))
            {
                if (node._m == 0)
                    return SymbolicFiniteAutomaton<S>.Epsilon;
                return SymbolicFiniteAutomaton<S>.Empty;
            }
            if (!description.ContainsKey(conditionFromSet))
                description[conditionFromSet] = RegexCharClass.SetDescription(str);
            SymbolicFiniteAutomaton<S> loopFromCondition = RegexToSFAGeneric<S>.CreateLoopFromCondition(minStateId, conditionFromSet, node._m, node._n);
            return ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private static SymbolicFiniteAutomaton<S> CreateLoopFromCondition(int minStateId, S cond, int m, int n)
        {
            if (m == 0 && n == int.MaxValue)
            {
                SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId }, new Move<S>[1] { Move<S>.T(minStateId, minStateId, cond) });
                sfa._isEpsilonFree = true;
                sfa._isDeterministic = true;
                return sfa;
            }
            if (m == 0 && n == 1)
            {
                SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[2] { minStateId, minStateId + 1 }, new Move<S>[1] { Move<S>.T(minStateId, minStateId + 1, cond) });
                sfa._isEpsilonFree = true;
                sfa._isDeterministic = true;
                return sfa;
            }
            if (n == int.MaxValue)
            {
                var moveList = new List<Move<S>>();
                for (int index = 0; index < m; ++index)
                    moveList.Add(Move<S>.T(minStateId + index, minStateId + index + 1, cond));
                moveList.Add(Move<S>.T(minStateId + m, minStateId + m, cond));
                SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(minStateId, new int[1] { minStateId + m }, moveList);
                sfa._isDeterministic = true;
                sfa._isEpsilonFree = true;
                return sfa;
            }
            var moveArray = new Move<S>[n];
            for (int index = 0; index < n; ++index)
                moveArray[index] = Move<S>.T(minStateId + index, minStateId + index + 1, cond);
            var numArray = new int[n + 1 - m];
            for (int index = m; index <= n; ++index)
                numArray[index - m] = index + minStateId;
            SymbolicFiniteAutomaton<S> sfa1 = SymbolicFiniteAutomaton<S>.Create(minStateId, numArray, moveArray);
            sfa1._isEpsilonFree = true;
            sfa1._isDeterministic = true;
            return sfa1;
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeECMABoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeGreedy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeGroup(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeLazyloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeBoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeNothing(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeNonboundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeNonECMABoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeNotonelazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeOnelazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodePrevent(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeRef(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeRequire(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeSetlazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeStart(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeTestgroup(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<S> ConvertNodeTestref(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private static bool IsNonDeterministic(SymbolicFiniteAutomaton<S> sfa)
        {
            return !sfa._isDeterministic;
        }

        private static bool HasEpsilons(SymbolicFiniteAutomaton<S> sfa)
        {
            return !sfa._isEpsilonFree;
        }

        private IEnumerable<Move<S>> GenerateMoves(Dictionary<Pair<int, int>, S> condMap, IEnumerable<Pair<int, int>> eMoves)
        {
            foreach (KeyValuePair<Pair<int, int>, S> cond in condMap)
                yield return Move<S>.T(cond.Key.First, cond.Key.Second, cond.Value);
            foreach (Pair<int, int> eMove in eMoves)
                yield return Move<S>.Epsilon(eMove.First, eMove.Second);
        }

        public void ToDot(SymbolicFiniteAutomaton<S> fa, string faName, string filename, DotRankDir rankdir, int fontsize)
        {
            var streamWriter = new StreamWriter(filename);
            ToDot(fa, faName, streamWriter, rankdir, fontsize);
            streamWriter.Close();
        }

        public void ToDot(SymbolicFiniteAutomaton<S> fa, string faName, TextWriter tw, DotRankDir rankdir, int fontsize)
        {
            tw.WriteLine("digraph \"" + faName + "\" {");
            tw.WriteLine(string.Format("rankdir={0};", rankdir.ToString()));
            tw.WriteLine();
            tw.WriteLine("//Initial state");
            tw.WriteLine(string.Format("node [style = filled, shape = ellipse, peripheries = {0}, fillcolor = \"#d3d3d3ff\", fontsize = {1}]", fa.IsFinalState(fa.InitialState) ? "2" : "1", fontsize));
            tw.WriteLine(fa.InitialState);
            tw.WriteLine();
            tw.WriteLine("//Final states");
            tw.WriteLine(string.Format("node [style = filled, shape = ellipse, peripheries = 2, fillcolor = white, fontsize = {0}]", fontsize));
            foreach (int finalState in fa.GetFinalStates())
            {
                if (finalState != fa.InitialState)
                    tw.WriteLine(finalState);
            }
            tw.WriteLine();
            tw.WriteLine("//Other states");
            tw.WriteLine(string.Format("node [style = filled, shape = ellipse, peripheries = 1, fillcolor = white, fontsize = {0}]", fontsize));
            foreach (int state in fa.States)
            {
                if (state != fa.InitialState && !fa.IsFinalState(state))
                    tw.WriteLine(state);
            }
            tw.WriteLine();
            tw.WriteLine("//Transitions");
            foreach (Move<S> move in fa.GetMoves())
                tw.WriteLine(string.Format("{0} -> {1} [label = \"{2}\"{3}, fontsize = {4} ];", move.SourceState, move.TargetState, move.IsEpsilon ? "" : description[move.Condition], move.IsEpsilon ? ", style = dashed" : "", fontsize));
            tw.WriteLine("}");
        }

        public void Display(SymbolicFiniteAutomaton<S> fa, string name, DotRankDir dir, int fontsize, bool showgraph, string format)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string str = string.Format("{1}\\{0}.dot", name, currentDirectory);
            string fileName = string.Format("{2}\\{0}.{1}", name, format, currentDirectory);
            var fileInfo1 = new FileInfo(str);
            if (fileInfo1.Exists)
                fileInfo1.IsReadOnly = false;
            var fileInfo2 = new FileInfo(fileName);
            if (fileInfo2.Exists)
                fileInfo2.IsReadOnly = false;
            ToDot(fa, name, str, dir, fontsize);
            var process = new Process();
            process.StartInfo = new ProcessStartInfo("dot.exe", string.Format("-T{2} {0} -o {1}", str, fileName, format));
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
    }
}
