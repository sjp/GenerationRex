using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using SJP.GenerationRex.RegularExpressions;

namespace SJP.GenerationRex
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
            this.description.Add(solver.True, "");
        }

        public SFA<S> Convert(string regex, RegexOptions options)
        {
            RegexOptions op = options & ~RegexOptions.RightToLeft;
            return this.ConvertNode(RegexParser.Parse(regex, op)._root, 0, true, true);
        }

        private SFA<S> ConvertNode(RegexNode node, int minStateId, bool isStart, bool isEnd)
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

        private SFA<S> ConvertNodeEmpty(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart && !isEnd)
                return SFA<S>.Epsilon;
            return SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId
            }, (IEnumerable<Move<S>>)new Move<S>[1]
            {
        Move<S>.To(minStateId, minStateId, this.solver.True)
            });
        }

        private SFA<S> ConvertNodeMulti(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            int length = str.Length;
            bool caseInsensitive = (node._options & RegexOptions.IgnoreCase) != RegexOptions.None;
            int num1 = minStateId;
            int num2 = num1 + length;
            int[] numArray = new int[1] { num2 };
            List<Move<S>> moveList = new List<Move<S>>();
            for (int index1 = 0; index1 < length; ++index1)
            {
                List<char[]> chArrayList = new List<char[]>();
                char c = str[index1];
                chArrayList.Add(new char[2] { c, c });
                S index2 = this.solver.MkRangesConstraint(caseInsensitive, (IEnumerable<char[]>)chArrayList);
                if (!this.description.ContainsKey(index2))
                    this.description[index2] = RexEngine.Escape(c);
                moveList.Add(Move<S>.To(num1 + index1, num1 + index1 + 1, index2));
            }
            SFA<S> sfa = SFA<S>.Create(num1, (IEnumerable<int>)numArray, (IEnumerable<Move<S>>)moveList);
            sfa.isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.To(num1, num1, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.To(num2, num2, this.solver.True));
            sfa.isEpsilonFree = true;
            return sfa;
        }

        private SFA<S> ConvertNodeNotone(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = this.solver.MkNot(this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch));
            if (!this.description.ContainsKey(index))
                this.description[index] = string.Format("[^{0}]", (object)RexEngine.Escape(node._ch));
            SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 1
            }, (IEnumerable<Move<S>>)new Move<S>[1]
            {
        Move<S>.To(minStateId, minStateId + 1, index)
            });
            sfa.isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.To(minStateId, minStateId, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.To(minStateId + 1, minStateId + 1, this.solver.True));
            return sfa;
        }

        private SFA<S> ConvertNodeOne(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch);
            if (!this.description.ContainsKey(index))
                this.description[index] = RexEngine.Escape(node._ch);
            SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 1
            }, (IEnumerable<Move<S>>)new Move<S>[1]
            {
        Move<S>.To(minStateId, minStateId + 1, index)
            });
            sfa.isEpsilonFree = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.To(minStateId, minStateId, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.To(minStateId + 1, minStateId + 1, this.solver.True));
            return sfa;
        }

        private SFA<S> ConvertNodeSet(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            S conditionFromSet = this.CreateConditionFromSet((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, str);
            if (conditionFromSet.Equals((object)this.solver.False))
                return SFA<S>.Empty;
            int num = minStateId + 1;
            SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        num
            }, (IEnumerable<Move<S>>)new Move<S>[1]
            {
        Move<S>.To(minStateId, num, conditionFromSet)
            });
            sfa.isDeterministic = true;
            if (isStart)
            {
                sfa.AddMove(Move<S>.To(minStateId, minStateId, this.solver.True));
                sfa.isDeterministic = false;
            }
            if (isEnd)
                sfa.AddMove(Move<S>.To(num, num, this.solver.True));
            sfa.isEpsilonFree = true;
            return sfa;
        }

        private S CreateConditionFromSet(bool ignoreCase, string set)
        {
            bool flag1 = RegexCharClass.IsNegated(set);
            List<S> sList = new List<S>();
            foreach (Pair<char, char> range in RegexToSFAGeneric<S>.ComputeRanges(set))
            {
                S constraint = this.solver.MkRangeConstraint(ignoreCase, range.First, range.Second);
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
                    S condition = this.MapCategoryCodeToCondition((int)Math.Abs(num4) - 1);
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
                        S condition = this.MapCategoryCodeSetToCondition(catCodes);
                        S s = flag1 ^ flag2 ? this.solver.MkNot(condition) : condition;
                        sList.Add(s);
                    }
                }
            }
            S constraint1 = default(S);
            if (set.Length > startIndex)
            {
                string set1 = set.Substring(startIndex);
                constraint1 = this.CreateConditionFromSet(ignoreCase, set1);
            }
            S constraint1_1 = sList.Count != 0 ? (flag1 ? this.solver.MkAnd((IEnumerable<S>)sList) : this.solver.MkOr((IEnumerable<S>)sList)) : (flag1 ? this.solver.False : this.solver.True);
            if ((object)constraint1 != null)
                constraint1_1 = this.solver.MkAnd(constraint1_1, this.solver.MkNot(constraint1));
            return constraint1_1;
        }

        private static List<Pair<char, char>> ComputeRanges(string set)
        {
            int capacity = (int)set[1];
            List<Pair<char, char>> pairList = new List<Pair<char, char>>(capacity);
            int index1 = 3;
            int num = index1 + capacity;
            while (index1 < num)
            {
                char first = set[index1];
                int index2 = index1 + 1;
                char second = index2 >= num ? char.MaxValue : (char)((uint)set[index2] - 1U);
                index1 = index2 + 1;
                pairList.Add(new Pair<char, char>(first, second));
            }
            return pairList;
        }

        private S MapCategoryCodeSetToCondition(HashSet<int> catCodes)
        {
            S constraint1 = default(S);
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
                S condition = this.MapCategoryCodeToCondition(catCode);
                constraint1 = (object)constraint1 == null ? condition : this.solver.MkOr(constraint1, condition);
            }
            return constraint1;
        }

        private S MapCategoryCodeToCondition(int code)
        {
            if (code == 99)
                return this.categorizer.WhiteSpaceCondition;
            if (code < 0 || code > 29)
                throw new ArgumentOutOfRangeException(nameof(code), "Must be in the range 0..29 or equal to 99");
            return this.categorizer.CategoryCondition(code);
        }

        private SFA<S> ConvertNodeEnd(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            if (!isStart)
                return SFA<S>.Epsilon;
            return SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId
            }, (IEnumerable<Move<S>>)new Move<S>[1]
            {
        Move<S>.To(minStateId, minStateId, this.solver.True)
            });
        }

        private SFA<S> ConvertNodeEndZ(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            if (!isStart)
                return SFA<S>.Epsilon;
            return SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId
            }, (IEnumerable<Move<S>>)new Move<S>[1]
            {
        Move<S>.To(minStateId, minStateId, this.solver.True)
            });
        }

        private SFA<S> ConvertNodeBeginning(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException("The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop");
            if (!isEnd)
                return SFA<S>.Epsilon;
            return SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId
            }, (IEnumerable<Move<S>>)new Move<S>[1]
            {
        Move<S>.To(minStateId, minStateId, this.solver.True)
            });
        }

        private SFA<S> ConvertNodeBol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isStart)
                throw new RexException("The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop");
            SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 2
            }, (IEnumerable<Move<S>>)new Move<S>[4]
            {
        Move<S>.Epsilon(minStateId, minStateId + 2),
        Move<S>.Epsilon(minStateId, minStateId + 1),
        Move<S>.To(minStateId + 1, minStateId + 1, this.solver.True),
        Move<S>.To(minStateId + 1, minStateId + 2, this.solver.MkCharConstraint(false, '\n'))
            });
            if (isEnd)
                sfa.AddMove(Move<S>.To(sfa.FinalState, sfa.FinalState, this.solver.True));
            return sfa;
        }

        private SFA<S> ConvertNodeEol(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            if (!isEnd)
                throw new RexException("The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop");
            SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
            {
        minStateId + 2
            }, (IEnumerable<Move<S>>)new Move<S>[4]
            {
        Move<S>.Epsilon(minStateId, minStateId + 2),
        Move<S>.Epsilon(minStateId + 1, minStateId + 2),
        Move<S>.To(minStateId + 1, minStateId + 1, this.solver.True),
        Move<S>.To(minStateId, minStateId + 1, this.solver.MkCharConstraint(false, '\n'))
            });
            if (isStart)
                sfa.AddMove(Move<S>.To(sfa.InitialState, sfa.InitialState, this.solver.True));
            return sfa;
        }

        private SFA<S> ConvertNodeAlternate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            List<SFA<S>> sfas = new List<SFA<S>>();
            int minStateId1 = minStateId + 1;
            bool addEmptyWord = false;
            foreach (RegexNode child in node._children)
            {
                SFA<S> sfa = this.ConvertNode(child, minStateId1, isStart, isEnd);
                if (sfa != SFA<S>.Empty)
                {
                    if (sfa == SFA<S>.Epsilon)
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

        private SFA<S> AlternateSFAs(int start, List<SFA<S>> sfas, bool addEmptyWord)
        {
            if (sfas.Count == 0)
            {
                if (addEmptyWord)
                    return SFA<S>.Epsilon;
                return SFA<S>.Empty;
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
            foreach (SFA<S> sfa in sfas)
            {
                if (!sfa.InitialStateIsSource)
                {
                    flag1 = false;
                    break;
                }
            }
            bool flag2 = !sfas.Exists(new Predicate<SFA<S>>(RegexToSFAGeneric<S>.IsNonDeterministic));
            sfas.Exists(new Predicate<SFA<S>>(RegexToSFAGeneric<S>.HasEpsilons));
            bool flag3 = true;
            int val2 = int.MinValue;
            foreach (SFA<S> sfa in sfas)
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
            Dictionary<Pair<int, int>, S> condMap = new Dictionary<Pair<int, int>, S>();
            HashSet<Pair<int, int>> pairSet = new HashSet<Pair<int, int>>();
            if (!flag1)
            {
                flag2 = false;
                foreach (SFA<S> sfa in sfas)
                    pairSet.Add(new Pair<int, int>(start, sfa.InitialState));
            }
            else if (flag2)
            {
                for (int index1 = 0; index1 < sfas.Count - 1; ++index1)
                {
                    for (int index2 = index1 + 1; index2 < sfas.Count; ++index2)
                    {
                        S constraint1 = this.solver.False;
                        foreach (Move<S> move in sfas[index1].GetMovesFrom(sfas[index1].InitialState))
                            constraint1 = this.solver.MkOr(constraint1, move.Condition);
                        S s = this.solver.False;
                        foreach (Move<S> move in sfas[index2].GetMovesFrom(sfas[index2].InitialState))
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
            foreach (SFA<S> sfa in sfas)
            {
                foreach (Move<S> move in sfa.GetMoves())
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
                        S constraint1;
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
            SFA<S> sfa1 = SFA<S>.Create(start, (IEnumerable<int>)intList, this.GenerateMoves(condMap, (IEnumerable<Pair<int, int>>)pairSet));
            sfa1.isDeterministic = flag2;
            return sfa1;
        }

        private SFA<S> ConvertNodeConcatenate(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            List<RegexNode> children = node._children;
            List<SFA<S>> sfas = new List<SFA<S>>();
            int minStateId1 = minStateId;
            for (int index = 0; index < children.Count; ++index)
            {
                SFA<S> sfa = this.ConvertNode(children[index], minStateId1, isStart && index == 0, isEnd && index == children.Count - 1);
                if (sfa == SFA<S>.Empty)
                    return SFA<S>.Empty;
                if (sfa != SFA<S>.Epsilon)
                {
                    sfas.Add(sfa);
                    minStateId1 = sfa.MaxState + 1;
                }
            }
            return this.ConcatenateSFAs(sfas);
        }

        private SFA<S> ConcatenateSFAs(List<SFA<S>> sfas)
        {
            if (sfas.Count == 0)
                return SFA<S>.Epsilon;
            if (sfas.Count == 1)
                return sfas[0];
            SFA<S> sfa = sfas[0];
            for (int index = 1; index < sfas.Count; ++index)
                sfa.Concat(sfas[index]);
            return sfa;
        }

        private SFA<S> ConvertNodeLoop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            SFA<S> sfa = this.ConvertNode(node._children[0], minStateId, false, false);
            int m = node._m;
            int n = node._n;
            SFA<S> loop;
            if (m == 0 && sfa.IsEmpty)
                loop = SFA<S>.Epsilon;
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
                    return SFA<S>.Empty;
                loop = sfa;
            }
            else if (n == int.MaxValue)
            {
                if (sfa.IsEmpty)
                    return SFA<S>.Empty;
                if (sfa.IsFinalState(sfa.InitialState))
                {
                    loop = this.MakeKleeneClosure(sfa);
                }
                else
                {
                    List<SFA<S>> sfas = new List<SFA<S>>();
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
                List<SFA<S>> sfas = new List<SFA<S>>();
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

        private SFA<S> ExtendLoop(int minStateId, bool isStart, bool isEnd, SFA<S> loop)
        {
            if (isStart)
            {
                if (loop != SFA<S>.Epsilon)
                {
                    SFA<S> sfa = SFA<S>.Create(loop.MaxState + 1, (IEnumerable<int>)new int[1]
                    {
            loop.MaxState + 1
                    }, (IEnumerable<Move<S>>)new Move<S>[1]
                    {
            Move<S>.To(loop.MaxState + 1, loop.MaxState + 1, this.solver.True)
                    });
                    sfa.Concat(loop);
                    loop = sfa;
                }
                else
                    loop = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
                    {
            minStateId
                    }, (IEnumerable<Move<S>>)new Move<S>[1]
                    {
            Move<S>.To(minStateId, minStateId, this.solver.True)
                    });
            }
            if (isEnd)
            {
                if (loop != SFA<S>.Epsilon)
                    loop.Concat(SFA<S>.Create(loop.MaxState + 1, (IEnumerable<int>)new int[1]
                    {
            loop.MaxState + 1
                    }, (IEnumerable<Move<S>>)new Move<S>[1]
                    {
            Move<S>.To(loop.MaxState + 1, loop.MaxState + 1, this.solver.True)
                    }));
                else
                    loop = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
                    {
            minStateId
                    }, (IEnumerable<Move<S>>)new Move<S>[1]
                    {
            Move<S>.To(minStateId, minStateId, this.solver.True)
                    });
            }
            return loop;
        }

        private SFA<S> MakeKleeneClosure(SFA<S> sfa)
        {
            if (sfa == SFA<S>.Empty || sfa == SFA<S>.Epsilon)
                return SFA<S>.Epsilon;
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
            return sfa.RemoveEpsilonLoops(new Func<S, S, S>(this.solver.MkOr));
        }

        private SFA<S> ConvertNodeNotoneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = this.solver.MkNot(this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch));
            if (!this.description.ContainsKey(index))
                this.description[index] = string.Format("[^{0}]", (object)RexEngine.Escape(node._ch));
            SFA<S> loopFromCondition = RegexToSFAGeneric<S>.CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return this.ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SFA<S> ConvertNodeOneloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            S index = this.solver.MkCharConstraint((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, node._ch);
            if (!this.description.ContainsKey(index))
                this.description[index] = string.Format("{0}", (object)RexEngine.Escape(node._ch));
            SFA<S> loopFromCondition = RegexToSFAGeneric<S>.CreateLoopFromCondition(minStateId, index, node._m, node._n);
            return this.ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private SFA<S> ConvertNodeSetloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            string str = node._str;
            S conditionFromSet = this.CreateConditionFromSet((node._options & RegexOptions.IgnoreCase) != RegexOptions.None, str);
            if (conditionFromSet.Equals((object)this.solver.False))
            {
                if (node._m == 0)
                    return SFA<S>.Epsilon;
                return SFA<S>.Empty;
            }
            SFA<S> loopFromCondition = RegexToSFAGeneric<S>.CreateLoopFromCondition(minStateId, conditionFromSet, node._m, node._n);
            return this.ExtendLoop(minStateId, isStart, isEnd, loopFromCondition);
        }

        private static SFA<S> CreateLoopFromCondition(int minStateId, S cond, int m, int n)
        {
            if (m == 0 && n == int.MaxValue)
            {
                SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
                {
          minStateId
                }, (IEnumerable<Move<S>>)new Move<S>[1]
                {
          Move<S>.To(minStateId, minStateId, cond)
                });
                sfa.isEpsilonFree = true;
                sfa.isDeterministic = true;
                return sfa;
            }
            if (m == 0 && n == 1)
            {
                SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[2]
                {
          minStateId,
          minStateId + 1
                }, (IEnumerable<Move<S>>)new Move<S>[1]
                {
          Move<S>.To(minStateId, minStateId + 1, cond)
                });
                sfa.isEpsilonFree = true;
                sfa.isDeterministic = true;
                return sfa;
            }
            if (n == int.MaxValue)
            {
                List<Move<S>> moveList = new List<Move<S>>();
                for (int index = 0; index < m; ++index)
                    moveList.Add(Move<S>.To(minStateId + index, minStateId + index + 1, cond));
                moveList.Add(Move<S>.To(minStateId + m, minStateId + m, cond));
                SFA<S> sfa = SFA<S>.Create(minStateId, (IEnumerable<int>)new int[1]
                {
          minStateId + m
                }, (IEnumerable<Move<S>>)moveList);
                sfa.isDeterministic = true;
                sfa.isEpsilonFree = true;
                return sfa;
            }
            Move<S>[] moveArray = new Move<S>[n];
            for (int index = 0; index < n; ++index)
                moveArray[index] = Move<S>.To(minStateId + index, minStateId + index + 1, cond);
            int[] numArray = new int[n + 1 - m];
            for (int index = m; index <= n; ++index)
                numArray[index - m] = index + minStateId;
            SFA<S> sfa1 = SFA<S>.Create(minStateId, (IEnumerable<int>)numArray, (IEnumerable<Move<S>>)moveArray);
            sfa1.isEpsilonFree = true;
            sfa1.isDeterministic = true;
            return sfa1;
        }

        private SFA<S> ConvertNodeECMABoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeGreedy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeGroup(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeLazyloop(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeBoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeNothing(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeNonboundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeNonECMABoundary(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeNotonelazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeOnelazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodePrevent(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeRef(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeRequire(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeSetlazy(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeStart(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeTestgroup(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private SFA<S> ConvertNodeTestref(RegexNode node, int minStateId, bool isStart, bool isEnd)
        {
            throw new RexException("The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution");
        }

        private static bool IsNonDeterministic(SFA<S> sfa)
        {
            return !sfa.isDeterministic;
        }

        private static bool HasEpsilons(SFA<S> sfa)
        {
            return !sfa.isEpsilonFree;
        }

        private IEnumerable<Move<S>> GenerateMoves(Dictionary<Pair<int, int>, S> condMap, IEnumerable<Pair<int, int>> eMoves)
        {
            foreach (KeyValuePair<Pair<int, int>, S> cond in condMap)
                yield return Move<S>.To(cond.Key.First, cond.Key.Second, cond.Value);
            foreach (Pair<int, int> eMove in eMoves)
                yield return Move<S>.Epsilon(eMove.First, eMove.Second);
        }

        public void ToDot(SFA<S> fa, string faName, string filename, DotRankDir rankdir, int fontsize)
        {
            StreamWriter streamWriter = new StreamWriter(filename);
            this.ToDot(fa, faName, (TextWriter)streamWriter, rankdir, fontsize);
            streamWriter.Close();
        }

        public void ToDot(SFA<S> fa, string faName, TextWriter tw, DotRankDir rankdir, int fontsize)
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
            foreach (Move<S> move in fa.GetMoves())
                tw.WriteLine(string.Format("{0} -> {1} [label = \"{2}\"{3}, fontsize = {4} ];", (object)move.SourceState, (object)move.TargetState, move.IsEpsilon ? (object)"" : (object)this.description[move.Condition], move.IsEpsilon ? (object)", style = dashed" : (object)"", (object)fontsize));
            tw.WriteLine("}");
        }

        public void Display(SFA<S> fa, string name, DotRankDir dir, int fontsize, bool showgraph, string format)
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
