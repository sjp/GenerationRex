using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using SJP.GenerationRex.RegularExpressions;

namespace SJP.GenerationRex
{
    internal class RegexToSFA<TConstraint>
    {
        private Dictionary<TConstraint, string> description = new Dictionary<TConstraint, string>();
        private const int SETLENGTH = 1;
        private const int CATEGORYLENGTH = 2;
        private const int SETSTART = 3;
        private const char Lastchar = '\xFFFF';
        private ICharacterConstraintSolver<TConstraint> solver;
        private IUnicodeCategoryConditions<TConstraint> categorizer;

        public RegexToSFA(ICharacterConstraintSolver<TConstraint> solver, IUnicodeCategoryConditions<TConstraint> categorizer)
        {
            this.solver = solver;
            this.categorizer = categorizer;
            this.description.Add(solver.True, "");
        }

        public SymbolicFiniteAutomaton<TConstraint> Convert(string regex, RegexOptions options)
        {
            RegexOptions op = options & ~RegexOptions.RightToLeft;
            return this.ConvertNode(RegexParser.Parse(regex, op).Root, 0, true, true);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNode(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            switch (node._type)
            {
                case 3:
                    return this.ConvertNodeOneloop(node, minStateId, isStart, isEnd);
                case 4:
                    return this.ConvertNodeNotoneloop(node, minStateId, isStart, isEnd);
                case 5:
                    return this.ConvertNodeSetloop(node, minStateId, isStart, isEnd);
                case 6:
                    return this.ConvertNodeOnelazy(node, minStateId, isStart, isEnd);
                case 7:
                    return this.ConvertNodeNotonelazy(node, minStateId, isStart, isEnd);
                case 8:
                    return this.ConvertNodeSetlazy(node, minStateId, isStart, isEnd);
                case 9:
                    return this.ConvertNodeOne(node, minStateId, isStart, isEnd);
                case 10:
                    return this.ConvertNodeNotone(node, minStateId, isStart, isEnd);
                case 11:
                    return this.ConvertNodeSet(node, minStateId, isStart, isEnd);
                case 12:
                    return this.ConvertNodeMulti(node, minStateId, isStart, isEnd);
                case 13:
                    return this.ConvertNodeRef(node, minStateId, isStart, isEnd);
                case 14:
                    return this.ConvertNodeBol(node, minStateId, isStart, isEnd);
                case 15:
                    return this.ConvertNodeEol(node, minStateId, isStart, isEnd);
                case 16:
                    return this.ConvertNodeBoundary(node, minStateId, isStart, isEnd);
                case 17:
                    return this.ConvertNodeNonboundary(node, minStateId, isStart, isEnd);
                case 18:
                    return this.ConvertNodeBeginning(node, minStateId, isStart, isEnd);
                case 19:
                    return this.ConvertNodeStart(node, minStateId, isStart, isEnd);
                case 20:
                    return this.ConvertNodeEndZ(node, minStateId, isStart, isEnd);
                case 21:
                    return this.ConvertNodeEnd(node, minStateId, isStart, isEnd);
                case 22:
                    return this.ConvertNodeNothing(node, minStateId, isStart, isEnd);
                case 23:
                    return this.ConvertNodeEmpty(node, minStateId, isStart, isEnd);
                case 24:
                    return this.ConvertNodeAlternate(node, minStateId, isStart, isEnd);
                case 25:
                    return this.ConvertNodeConcatenate(node, minStateId, isStart, isEnd);
                case 26:
                    return this.ConvertNodeLoop(node, minStateId, isStart, isEnd);
                case 27:
                    return this.ConvertNodeLazyloop(node, minStateId, isStart, isEnd);
                case 28:
                    return this.ConvertNode(node.Child(0), minStateId, isStart, isEnd);
                case 29:
                    return this.ConvertNodeGroup(node, minStateId, isStart, isEnd);
                case 30:
                    return this.ConvertNodeRequire(node, minStateId, isStart, isEnd);
                case 31:
                    return this.ConvertNodePrevent(node, minStateId, isStart, isEnd);
                case 32:
                    return this.ConvertNodeGreedy(node, minStateId, isStart, isEnd);
                case 33:
                    return this.ConvertNodeTestref(node, minStateId, isStart, isEnd);
                case 34:
                    return this.ConvertNodeTestgroup(node, minStateId, isStart, isEnd);
                case 41:
                    return this.ConvertNodeECMABoundary(node, minStateId, isStart, isEnd);
                case 42:
                    return this.ConvertNodeNonECMABoundary(node, minStateId, isStart, isEnd);
                default:
                    throw new RexException("Unrecognized regex construct");
            }
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEmpty(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart && !isEnd)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            return SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId
            }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
            {
        Move<TConstraint>.To(minStateId, minStateId, this.solver.True)
            });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeMulti(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            int length = str.Length;
            bool caseInsensitive = (node._options & RegexOptions.IgnoreCase) != RegexOptions.None;
            int num1 = minStateId;
            int num2 = num1 + length;
            int[] numArray = new int[1] { num2 };
            List<Move<TConstraint>> moveList = new List<Move<TConstraint>>();
            for (int index1 = 0; index1 < length; ++index1)
            {
                List<char[]> chArrayList = new List<char[]>();
                char c = str[index1];
                chArrayList.Add(new char[2] { c, c });
                TConstraint index2 = this.solver.MkRangesConstraint(caseInsensitive, (IEnumerable<char[]>)chArrayList);
                if (!this.description.ContainsKey(index2))
                    this.description[index2] = RexEngine.Escape(c);
                moveList.Add(Move<TConstraint>.To(num1 + index1, num1 + index1 + 1, index2));
            }
            SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(num1, (IEnumerable<int>)numArray, (IEnumerable<Move<TConstraint>>)moveList);
            sfa.isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(num1, num1, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(num2, num2, this.solver.True));
            sfa.isEpsilonFree = true;
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNotone(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            TConstraint index = this.solver.MkNot(this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch));
            if (!this.description.ContainsKey(index))
                this.description[index] = string.Format("[^{0}]", (object)RexEngine.Escape(node._ch));
            SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 1
            }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
            {
        Move<TConstraint>.To(minStateId, minStateId + 1, index)
            });
            sfa.isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(minStateId, minStateId, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(minStateId + 1, minStateId + 1, this.solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeOne(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            TConstraint index = this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch);
            if (!this.description.ContainsKey(index))
                this.description[index] = RexEngine.Escape(node._ch);
            SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 1
            }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
            {
        Move<TConstraint>.To(minStateId, minStateId + 1, index)
            });
            sfa.isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(minStateId, minStateId, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(minStateId + 1, minStateId + 1, this.solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeSet(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            TConstraint conditionFromSet = this.CreateConditionFromSet((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, str);
            if (conditionFromSet.Equals((object)this.solver.False))
                return SymbolicFiniteAutomaton<TConstraint>.Empty;
            int num = minStateId + 1;
            SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        num
            }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
            {
        Move<TConstraint>.To(minStateId, num, conditionFromSet)
            });
            sfa.isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<TConstraint>.To(minStateId, minStateId, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(num, num, this.solver.True));
            sfa.isEpsilonFree = true;
            return sfa;
        }

        private TConstraint CreateConditionFromSet(bool ignoreCase, string set)
        {
            bool flag1 = RegexCharClass.IsNegated(set);
            List<TConstraint> sList = new List<TConstraint>();
            foreach (Pair<char, char> range in RegexToSFA<TConstraint>.ComputeRanges(set))
            {
                TConstraint constraint = this.solver.MkRangeConstraint(ignoreCase, range.First, range.Second);
                sList.Add(flag1 ? this.solver.MkNot(constraint) : constraint);
            }
            int num1 = (int)set[1];
            int num2 = (int)set[2];
            int num3 = num1 + 3;
            int startIndex = num3;
            while (startIndex < num3 + num2)
            {
                short num4 = (short)set[startIndex++];
                if ((int)num4 != 0)
                {
                    TConstraint condition = this.MapCategoryCodeToCondition((int)Math.Abs(num4) - 1);
                    sList.Add((int)num4 < 0 ^ flag1 ? this.solver.MkNot(condition) : condition);
                }
                else
                {
                    short num5 = (short)set[startIndex++];
                    if ((int)num5 != 0)
                    {
                        HashSet<int> catCodes = new HashSet<int>();
                        bool flag2 = (int)num5 < 0;
                        for (; (int)num5 != 0; num5 = (short)set[startIndex++])
                            catCodes.Add((int)Math.Abs(num5) - 1);
                        TConstraint condition = this.MapCategoryCodeSetToCondition(catCodes);
                        TConstraint s = flag1 ^ flag2 ? this.solver.MkNot(condition) : condition;
                        sList.Add(s);
                    }
                }
            }
            TConstraint constraint1 = default(TConstraint);
            if (set.Length > startIndex)
            {
                string set1 = set.Substring(startIndex);
                constraint1 = this.CreateConditionFromSet(ignoreCase, set1);
            }
            TConstraint constraint1_1 = sList.Count != 0 ? (flag1 ? this.solver.MkAnd((IEnumerable<TConstraint>)sList) : this.solver.MkOr((IEnumerable<TConstraint>)sList)) : (flag1 ? this.solver.False : this.solver.True);
            if ((object)constraint1 != null)
                constraint1_1 = this.solver.MkAnd(constraint1_1, this.solver.MkNot(constraint1));
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

        private TConstraint MapCategoryCodeSetToCondition(HashSet<int> catCodes)
        {
            TConstraint constraint1 = default(TConstraint);
            if (catCodes.Contains(0) && catCodes.Contains(1) && (catCodes.Contains(2) && catCodes.Contains(3)) && (catCodes.Contains(4) && catCodes.Contains(8) && catCodes.Contains(18)))
            {
                catCodes.Remove(0);
                catCodes.Remove(1);
                catCodes.Remove(2);
                catCodes.Remove(3);
                catCodes.Remove(4);
                catCodes.Remove(8);
                catCodes.Remove(18);
                constraint1 = this.categorizer.WordLetterCondition;
            }
            foreach (int catCode in catCodes)
            {
                TConstraint condition = this.MapCategoryCodeToCondition(catCode);
                constraint1 = (object)constraint1 == null ? condition : this.solver.MkOr(constraint1, condition);
            }
            return constraint1;
        }

        private TConstraint MapCategoryCodeToCondition(int code)
        {
            if (code == 99)
                return this.categorizer.WhiteSpaceCondition;
            if (code < 0 || code > 29)
                throw new ArgumentOutOfRangeException(nameof(code), "Must be in the range 0..29 or equal to 99");
            return this.categorizer.CategoryCondition(code);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEnd(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            if (!isStart)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            return SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, solver.True) });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEndZ(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            if (!isStart)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            return SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, solver.True) });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeBeginning(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException("The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop");
            if (!isEnd)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            return SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, new[] { minStateId }, new[] { Move<TConstraint>.To(minStateId, minStateId, solver.True) });
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeBol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException("The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop");
            SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 2
            }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[4]
            {
        Move<TConstraint>.Epsilon(minStateId, minStateId + 2),
        Move<TConstraint>.Epsilon(minStateId, minStateId + 1),
        Move<TConstraint>.To(minStateId + 1, minStateId + 1, this.solver.True),
        Move<TConstraint>.To(minStateId + 1, minStateId + 2, this.solver.MkCharConstraint(false, '\n'))
            });
            if (isEnd)
                sfa.AddMove(Move<TConstraint>.To(sfa.FinalState, sfa.FinalState, this.solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeEol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 2
            }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[4]
            {
        Move<TConstraint>.Epsilon(minStateId, minStateId + 2),
        Move<TConstraint>.Epsilon(minStateId + 1, minStateId + 2),
        Move<TConstraint>.To(minStateId + 1, minStateId + 1, this.solver.True),
        Move<TConstraint>.To(minStateId, minStateId + 1, this.solver.MkCharConstraint(false, '\n'))
            });
            if (isStart)
                sfa.AddMove(Move<TConstraint>.To(sfa.InitialState, sfa.InitialState, this.solver.True));
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeAlternate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            List<SymbolicFiniteAutomaton<TConstraint>> sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
            int minStateId1 = minStateId + 1;
            bool addEmptyWord = false;
            foreach (RegexNode child in node._children)
            {
                SymbolicFiniteAutomaton<TConstraint> sfa = this.ConvertNode(child, minStateId1, isStart, isEnd);
                if (sfa != SymbolicFiniteAutomaton<TConstraint>.Empty)
                {
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
            }
            return this.AlternateSFAs(minStateId, sfas, addEmptyWord);
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
            List<int> intList = new List<int>();
            if (addEmptyWord)
                intList.Add(start);
            Dictionary<Pair<int, int>, TConstraint> condMap = new Dictionary<Pair<int, int>, TConstraint>();
            HashSet<Pair<int, int>> pairSet = new HashSet<Pair<int, int>>();
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
                        TConstraint constraint1 = this.solver.False;
                        foreach (Move<TConstraint> move in sfas[index1].GetMovesFrom(sfas[index1].InitialState))
                            constraint1 = this.solver.MkOr(constraint1, move.Condition);
                        TConstraint s = this.solver.False;
                        foreach (Move<TConstraint> move in sfas[index2].GetMovesFrom(sfas[index2].InitialState))
                            s = this.solver.MkOr(s, move.Condition);
                        flag2 = this.solver.MkAnd(constraint1, s).Equals((object)this.solver.False);
                        if (!flag2)
                            break;
                    }
                    if (!flag2)
                        break;
                }
            }
            if (flag3)
                intList.Add(val2);
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            foreach (SymbolicFiniteAutomaton<TConstraint> sfa in sfas)
            {
                foreach (Move<TConstraint> move in sfa.GetMoves())
                {
                    int first = !flag1 || sfa.InitialState != move.SourceState ? move.SourceState : start;
                    int second = !flag3 || sfa.FinalState != move.TargetState ? move.TargetState : val2;
                    Pair<int, int> key = new Pair<int, int>(first, second);
                    dictionary[move.SourceState] = first;
                    dictionary[move.TargetState] = second;
                    if (move.IsEpsilon)
                    {
                        if (first != second)
                            pairSet.Add(new Pair<int, int>(first, second));
                    }
                    else
                    {
                        TConstraint constraint1;
                        condMap[key] = !condMap.TryGetValue(key, out constraint1) ? move.Condition : this.solver.MkOr(constraint1, move.Condition);
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
            SymbolicFiniteAutomaton<TConstraint> sfa1 = SymbolicFiniteAutomaton<TConstraint>.Create(start, (IEnumerable<int>)intList, this.GenerateMoves(condMap, (IEnumerable<Pair<int, int>>)pairSet));
            sfa1.isDeterministic = flag2;
            return sfa1;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeConcatenate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            List<RegexNode> children = node._children;
            List<SymbolicFiniteAutomaton<TConstraint>> sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
            int minStateId1 = minStateId;
            for (int index = 0; index < children.Count; ++index)
            {
                SymbolicFiniteAutomaton<TConstraint> sfa = this.ConvertNode(children[index], minStateId1, isStart && index == 0, isEnd && index == children.Count - 1);
                if (sfa == SymbolicFiniteAutomaton<TConstraint>.Empty)
                    return SymbolicFiniteAutomaton<TConstraint>.Empty;
                if (sfa != SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                {
                    sfas.Add(sfa);
                    minStateId1 = sfa.MaxState + 1;
                }
            }
            return this.ConcatenateSFAs(sfas);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConcatenateSFAs(List<SymbolicFiniteAutomaton<TConstraint>> sfas)
        {
            if (sfas.Count == 0)
                return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            if (sfas.Count == 1)
                return sfas[0];
            SymbolicFiniteAutomaton<TConstraint> sfa = sfas[0];
            for (int index = 1; index < sfas.Count; ++index)
                sfa.Concat(sfas[index]);
            return sfa;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeLoop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            SymbolicFiniteAutomaton<TConstraint> sfa = this.ConvertNode(node._children[0], minStateId, false, false);
            int m = node._m;
            int n = node._n;
            SymbolicFiniteAutomaton<TConstraint> loop;
            if (m == 0 && sfa.IsEmpty)
                loop = SymbolicFiniteAutomaton<TConstraint>.Epsilon;
            else if (m == 0 && n == int.MaxValue)
                loop = this.MakeKleeneClosure(sfa);
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
                    loop = this.MakeKleeneClosure(sfa);
                }
                else
                {
                    List<SymbolicFiniteAutomaton<TConstraint>> sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
                    for (int index = 0; index < m; ++index)
                    {
                        sfas.Add(sfa);
                        sfa = sfa.MakeCopy(sfa.MaxState + 1);
                    }
                    sfas.Add(this.MakeKleeneClosure(sfa));
                    loop = this.ConcatenateSFAs(sfas);
                }
            }
            else
            {
                List<SymbolicFiniteAutomaton<TConstraint>> sfas = new List<SymbolicFiniteAutomaton<TConstraint>>();
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
                loop = this.ConcatenateSFAs(sfas);
            }
            return this.ExtendLoop(minStateId, isStart, isEnd, loop);
        }

        private SymbolicFiniteAutomaton<TConstraint> ExtendLoop(int minStateId, bool isStart, bool isEnd, SymbolicFiniteAutomaton<TConstraint> loop)
        {
            if (isStart)
            {
                if (loop != SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                {
                    SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(loop.MaxState + 1, (IEnumerable<int>)new int[1]
                    {
            loop.MaxState + 1
                    }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
                    {
            Move<TConstraint>.To(loop.MaxState + 1, loop.MaxState + 1, this.solver.True)
                    });
                    sfa.Concat(loop);
                    loop = sfa;
                }
                else
                    loop = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
                    {
            minStateId
                    }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
                    {
            Move<TConstraint>.To(minStateId, minStateId, this.solver.True)
                    });
            }
            if (isEnd)
            {
                if (loop != SymbolicFiniteAutomaton<TConstraint>.Epsilon)
                    loop.Concat(SymbolicFiniteAutomaton<TConstraint>.Create(loop.MaxState + 1, (IEnumerable<int>)new int[1]
                    {
            loop.MaxState + 1
                    }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
                    {
            Move<TConstraint>.To(loop.MaxState + 1, loop.MaxState + 1, this.solver.True)
                    }));
                else
                    loop = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
                    {
            minStateId
                    }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
                    {
            Move<TConstraint>.To(minStateId, minStateId, this.solver.True)
                    });
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
            return sfa.RemoveEpsilonLoops(new Func<TConstraint, TConstraint, TConstraint>(this.solver.MkOr));
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNotoneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            TConstraint index = this.solver.MkNot(this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch));
            if (!this.description.ContainsKey(index))
                this.description[index] = string.Format("[^{0}]", (object)RexEngine.Escape(node._ch));
            SymbolicFiniteAutomaton<TConstraint> loopFromCondition = RegexToSFA<TConstraint>.CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return this.ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeOneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            TConstraint index = this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch);
            if (!this.description.ContainsKey(index))
                this.description[index] = string.Format("{0}", (object)RexEngine.Escape(node._ch));
            SymbolicFiniteAutomaton<TConstraint> loopFromCondition = RegexToSFA<TConstraint>.CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return this.ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeSetloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            TConstraint conditionFromSet = this.CreateConditionFromSet((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, str);
            if (conditionFromSet.Equals((object)this.solver.False))
            {
                if (node._m == 0)
                    return SymbolicFiniteAutomaton<TConstraint>.Epsilon;
                return SymbolicFiniteAutomaton<TConstraint>.Empty;
            }
            SymbolicFiniteAutomaton<TConstraint> loopFromCondition = RegexToSFA<TConstraint>.CreateLoopFromCondition(minStateId, conditionFromSet, node._m, node._n);
            return this.ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private static SymbolicFiniteAutomaton<TConstraint> CreateLoopFromCondition(int minStateId, TConstraint cond, int m, int n)
        {
            if (m == 0 && n == int.MaxValue)
            {
                SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
                {
          minStateId
                }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
                {
          Move<TConstraint>.To(minStateId, minStateId, cond)
                });
                sfa.isEpsilonFree = true;
                sfa.isDeterministic = true;
                return sfa;
            }
            if (m == 0 && n == 1)
            {
                SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[2]
                {
          minStateId,
          minStateId + 1
                }, (IEnumerable<Move<TConstraint>>)new Move<TConstraint>[1]
                {
          Move<TConstraint>.To(minStateId, minStateId + 1, cond)
                });
                sfa.isEpsilonFree = true;
                sfa.isDeterministic = true;
                return sfa;
            }
            if (n == int.MaxValue)
            {
                List<Move<TConstraint>> moveList = new List<Move<TConstraint>>();
                for (int index = 0; index < m; ++index)
                    moveList.Add(Move<TConstraint>.To(minStateId + index, minStateId + index + 1, cond));
                moveList.Add(Move<TConstraint>.To(minStateId + m, minStateId + m, cond));
                SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)new int[1]
                {
          minStateId + m
                }, (IEnumerable<Move<TConstraint>>)moveList);
                sfa.isDeterministic = true;
                sfa.isEpsilonFree = true;
                return sfa;
            }
            Move<TConstraint>[] moveArray = new Move<TConstraint>[n];
            for (int index = 0; index < n; ++index)
                moveArray[index] = Move<TConstraint>.To(minStateId + index, minStateId + index + 1, cond);
            int[] numArray = new int[n + 1 - m];
            for (int index = m; index <= n; ++index)
                numArray[index - m] = index + minStateId;
            SymbolicFiniteAutomaton<TConstraint> sfa1 = SymbolicFiniteAutomaton<TConstraint>.Create(minStateId, (IEnumerable<int>)numArray, (IEnumerable<Move<TConstraint>>)moveArray);
            sfa1.isEpsilonFree = true;
            sfa1.isDeterministic = true;
            return sfa1;
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeECMABoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeGreedy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeGroup(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeLazyloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeBoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNothing(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNonboundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNonECMABoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeNotonelazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeOnelazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodePrevent(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeRef(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeRequire(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeSetlazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeStart(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeTestgroup(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SymbolicFiniteAutomaton<TConstraint> ConvertNodeTestref(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private static bool IsNonDeterministic(SymbolicFiniteAutomaton<TConstraint> sfa)
        {
            return !sfa.isDeterministic;
        }

        private static bool HasEpsilons(SymbolicFiniteAutomaton<TConstraint> sfa)
        {
            return !sfa.isEpsilonFree;
        }

        private IEnumerable<Move<TConstraint>> GenerateMoves(Dictionary<Pair<int, int>, TConstraint> condMap, IEnumerable<Pair<int, int>> eMoves)
        {
            foreach (KeyValuePair<Pair<int, int>, TConstraint> cond in condMap)
                yield return Move<TConstraint>.To(cond.Key.First, cond.Key.Second, cond.Value);
            foreach (Pair<int, int> eMove in eMoves)
                yield return Move<TConstraint>.Epsilon(eMove.First, eMove.Second);
        }

        public void ToDot(SymbolicFiniteAutomaton<TConstraint> fa, string faName, string filename, DotRankDir rankdir, int fontsize)
        {
            StreamWriter streamWriter = new StreamWriter(filename);
            this.ToDot(fa, faName, (TextWriter)streamWriter, rankdir, fontsize);
            streamWriter.Close();
        }

        public void ToDot(SymbolicFiniteAutomaton<TConstraint> fa, string faName, TextWriter tw, DotRankDir rankdir, int fontsize)
        {
            tw.WriteLine("digraph \"" + faName + "\" {");
            tw.WriteLine(string.Format("rankdir={0};", (object)rankdir.ToString()));
            tw.WriteLine();
            tw.WriteLine("//Initial state");
            tw.WriteLine(string.Format("node [style = filled, shape = ellipse, peripheries = {0}, fillcolor = \"#d3d3d3ff\", fontsize = {1}]", fa.IsFinalState(fa.InitialState) ? (object)"2" : (object)"1", (object)fontsize));
            tw.WriteLine(fa.InitialState);
            tw.WriteLine();
            tw.WriteLine("//Final states");
            tw.WriteLine(string.Format("node [style = filled, shape = ellipse, peripheries = 2, fillcolor = white, fontsize = {0}]", (object)fontsize));
            foreach (int finalState in fa.GetFinalStates())
            {
                if (finalState != fa.InitialState)
                    tw.WriteLine(finalState);
            }
            tw.WriteLine();
            tw.WriteLine("//Other states");
            tw.WriteLine(string.Format("node [style = filled, shape = ellipse, peripheries = 1, fillcolor = white, fontsize = {0}]", (object)fontsize));
            foreach (int state in fa.States)
            {
                if (state != fa.InitialState && !fa.IsFinalState(state))
                    tw.WriteLine(state);
            }
            tw.WriteLine();
            tw.WriteLine("//Transitions");
            foreach (Move<TConstraint> move in fa.GetMoves())
                tw.WriteLine(string.Format("{0} -> {1} [label = \"{2}\"{3}, fontsize = {4} ];", (object)move.SourceState, (object)move.TargetState, move.IsEpsilon ? (object)"" : (object)this.description[move.Condition], move.IsEpsilon ? (object)", style = dashed" : (object)"", (object)fontsize));
            tw.WriteLine("}");
        }

        public void Display(SymbolicFiniteAutomaton<TConstraint> fa, string name, DotRankDir dir, int fontsize, bool showgraph, string format)
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
            this.ToDot(fa, name, str, dir, fontsize);
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
    }
}
