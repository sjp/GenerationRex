using System;
using System.Collections.Generic;

namespace SJP.GenerationRex
{
    internal class SymbolicFiniteAutomaton<TConstraint>
    {
        public static SymbolicFiniteAutomaton<TConstraint> Empty = Create(0, new int[0], new Move<TConstraint>[0]);
        public static SymbolicFiniteAutomaton<TConstraint> Epsilon = Create(0, new int[1], new Move<TConstraint>[0]);
        private Dictionary<int, List<Move<TConstraint>>> delta;
        private Dictionary<int, List<Move<TConstraint>>> deltaInv;
        private int initialState;
        private HashSet<int> finalStateSet;
        private int maxState;
        internal bool isEpsilonFree;
        internal bool isDeterministic;

        public int FinalState
        {
            get
            {
                using (HashSet<int>.Enumerator enumerator = this.finalStateSet.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        return enumerator.Current;
                }
                throw new RexException("There is no final state");
            }
        }

        public bool HasMoreThanOneFinalState
        {
            get
            {
                return this.finalStateSet.Count > 1;
            }
        }

        public int OutDegree(int state)
        {
            return this.delta[state].Count;
        }

        public Move<TConstraint> GetMoveFrom(int state)
        {
            return this.delta[state][0];
        }

        public bool InitialStateIsSource
        {
            get
            {
                return this.deltaInv[this.initialState].Count == 0;
            }
        }

        public bool IsEpsilonFree
        {
            get
            {
                return this.isEpsilonFree;
            }
        }

        public int MoveCount
        {
            get
            {
                int num = 0;
                foreach (int key in this.delta.Keys)
                    num += this.delta[key].Count;
                return num;
            }
        }

        public bool IsDeterministic
        {
            get
            {
                return this.isDeterministic;
            }
        }

        public IEnumerable<int> GetEpsilonClosure(int state)
        {
            Stack<int> stack = new Stack<int>();
            HashSet<int> done = new HashSet<int>();
            done.Add(state);
            stack.Push(state);
            while (stack.Count > 0)
            {
                int s = stack.Pop();
                yield return s;
                foreach (Move<TConstraint> move in this.delta[s])
                {
                    if (move.IsEpsilon && !done.Contains(move.TargetState))
                    {
                        done.Add(move.TargetState);
                        stack.Push(move.TargetState);
                    }
                }
            }
        }

        public IEnumerable<int> GetInvEpsilonClosure(int state)
        {
            Stack<int> stack = new Stack<int>();
            HashSet<int> done = new HashSet<int>();
            done.Add(state);
            stack.Push(state);
            while (stack.Count > 0)
            {
                int s = stack.Pop();
                yield return s;
                foreach (Move<TConstraint> move in this.deltaInv[s])
                {
                    if (move.IsEpsilon && !done.Contains(move.SourceState))
                    {
                        done.Add(move.SourceState);
                        stack.Push(move.SourceState);
                    }
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this.finalStateSet.Count == 0;
            }
        }

        public static SymbolicFiniteAutomaton<TConstraint> Create(int initialState, IEnumerable<int> finalStates, IEnumerable<Move<TConstraint>> moves)
        {
            Dictionary<int, List<Move<TConstraint>>> dictionary1 = new Dictionary<int, List<Move<TConstraint>>>();
            Dictionary<int, List<Move<TConstraint>>> dictionary2 = new Dictionary<int, List<Move<TConstraint>>>();
            dictionary1[initialState] = new List<Move<TConstraint>>();
            dictionary2[initialState] = new List<Move<TConstraint>>();
            bool flag1 = true;
            int val1 = initialState;
            bool flag2 = true;
            foreach (Move<TConstraint> move in moves)
            {
                if (move.IsEpsilon)
                    flag1 = false;
                if (!dictionary1.ContainsKey(move.SourceState))
                    dictionary1[move.SourceState] = new List<Move<TConstraint>>();
                if (!dictionary1.ContainsKey(move.TargetState))
                    dictionary1[move.TargetState] = new List<Move<TConstraint>>();
                if (!dictionary2.ContainsKey(move.SourceState))
                    dictionary2[move.SourceState] = new List<Move<TConstraint>>();
                if (!dictionary2.ContainsKey(move.TargetState))
                    dictionary2[move.TargetState] = new List<Move<TConstraint>>();
                dictionary1[move.SourceState].Add(move);
                dictionary2[move.TargetState].Add(move);
                flag2 = flag2 && dictionary1[move.SourceState].Count < 2;
                val1 = Math.Max(val1, Math.Max(move.SourceState, move.TargetState));
            }
            HashSet<int> intSet = new HashSet<int>(finalStates);
            if (!intSet.IsSubsetOf((IEnumerable<int>)dictionary1.Keys))
                throw new RexException("The set of final states must be a subset of all states");
            return new SymbolicFiniteAutomaton<TConstraint>()
            {
                initialState = initialState,
                finalStateSet = intSet,
                isEpsilonFree = flag1,
                maxState = val1,
                delta = dictionary1,
                deltaInv = dictionary2,
                isDeterministic = flag2
            };
        }

        private SymbolicFiniteAutomaton()
        {
        }

        public int InitialState
        {
            get
            {
                return this.initialState;
            }
        }

        public int MaxState
        {
            get
            {
                return this.maxState;
            }
        }

        public IEnumerable<int> States
        {
            get
            {
                return (IEnumerable<int>)this.delta.Keys;
            }
        }

        public int StateCount
        {
            get
            {
                return this.delta.Count;
            }
        }

        public IEnumerable<Move<TConstraint>> GetMoves()
        {
            foreach (int state in this.States)
            {
                foreach (Move<TConstraint> move in this.delta[state])
                    yield return move;
            }
        }

        public IEnumerable<Move<TConstraint>> GetEpsilonMoves()
        {
            foreach (int state in this.States)
            {
                foreach (Move<TConstraint> move in this.delta[state])
                {
                    if (move.IsEpsilon)
                        yield return move;
                }
            }
        }

        public IEnumerable<int> GetEpsilonTargetsFrom(int state)
        {
            foreach (Move<TConstraint> move in this.delta[state])
            {
                if (move.IsEpsilon)
                    yield return move.TargetState;
            }
        }

        public IEnumerable<int> GetFinalStates()
        {
            return (IEnumerable<int>)this.finalStateSet;
        }

        public IEnumerable<Move<TConstraint>> GetMovesFrom(int sourceState)
        {
            return (IEnumerable<Move<TConstraint>>)this.delta[sourceState];
        }

        public int GetMovesCountFrom(int sourceState)
        {
            return this.delta[sourceState].Count;
        }

        public Move<TConstraint> GetNthMoveFrom(int sourceState, int n)
        {
            return this.delta[sourceState][n];
        }

        public IEnumerable<Move<TConstraint>> GetMovesFromStates(IEnumerable<int> sourceStates)
        {
            foreach (int sourceState in sourceStates)
            {
                foreach (Move<TConstraint> move in this.delta[sourceState])
                    yield return move;
            }
        }

        public bool IsFinalState(int state)
        {
            return this.finalStateSet.Contains(state);
        }

        public bool HasSingleFinalSink
        {
            get
            {
                if (this.finalStateSet.Count == 1)
                    return this.delta[this.FinalState].Count == 0;
                return false;
            }
        }

        public void Concat(SymbolicFiniteAutomaton<TConstraint> fa)
        {
            foreach (int state in fa.States)
            {
                this.delta[state] = new List<Move<TConstraint>>((IEnumerable<Move<TConstraint>>)fa.delta[state]);
                this.deltaInv[state] = new List<Move<TConstraint>>((IEnumerable<Move<TConstraint>>)fa.deltaInv[state]);
            }
            if (this.HasSingleFinalSink)
            {
                foreach (int finalState in this.finalStateSet)
                {
                    foreach (Move<TConstraint> move1 in this.deltaInv[finalState])
                    {
                        this.delta[move1.SourceState].Remove(move1);
                        Move<TConstraint> move2 = Move<TConstraint>.To(move1.SourceState == finalState ? fa.InitialState : move1.SourceState, fa.InitialState, move1.Condition);
                        this.delta[move2.SourceState].Add(move2);
                        this.deltaInv[move2.TargetState].Add(move2);
                    }
                    this.delta.Remove(finalState);
                    this.deltaInv.Remove(finalState);
                }
                if (this.finalStateSet.Contains(this.initialState))
                    this.initialState = fa.initialState;
                this.isEpsilonFree = this.isEpsilonFree && fa.isEpsilonFree;
                this.isDeterministic = this.isDeterministic && fa.isDeterministic;
            }
            else
            {
                foreach (int finalState in this.finalStateSet)
                {
                    Move<TConstraint> move = Move<TConstraint>.Epsilon(finalState, fa.initialState);
                    this.delta[finalState].Add(move);
                    this.deltaInv[fa.initialState].Add(move);
                }
                this.isEpsilonFree = false;
                this.isDeterministic = false;
            }
            this.finalStateSet = fa.finalStateSet;
            this.maxState = Math.Max(this.maxState, fa.maxState);
        }

        private bool AllFinalStatesAreSinks
        {
            get
            {
                foreach (int finalState in this.finalStateSet)
                {
                    if (this.delta[finalState].Count > 0)
                        return false;
                }
                return true;
            }
        }

        internal void SetFinalStates(IEnumerable<int> newFinalStates)
        {
            this.finalStateSet = new HashSet<int>(newFinalStates);
        }

        internal void MakeInitialStateFinal()
        {
            this.finalStateSet.Add(this.initialState);
        }

        internal void AddMove(Move<TConstraint> move)
        {
            if (!this.delta.ContainsKey(move.SourceState))
                this.delta[move.SourceState] = new List<Move<TConstraint>>();
            if (!this.deltaInv.ContainsKey(move.TargetState))
                this.deltaInv[move.TargetState] = new List<Move<TConstraint>>();
            this.delta[move.SourceState].Add(move);
            this.deltaInv[move.TargetState].Add(move);
            this.maxState = Math.Max(this.maxState, Math.Max(move.SourceState, move.TargetState));
            this.isEpsilonFree = this.isEpsilonFree && !move.IsEpsilon;
            this.isDeterministic = false;
        }

        public bool IsKleeneClosure()
        {
            if (!this.IsFinalState(this.initialState))
                return false;
            foreach (int finalState in this.finalStateSet)
            {
                if (finalState != this.initialState && !this.delta[finalState].Exists(new Predicate<Move<TConstraint>>(this.IsEpsilonMoveToInitialState)))
                    return false;
            }
            return true;
        }

        internal bool IsEpsilonMoveToInitialState(Move<TConstraint> move)
        {
            if (move.IsEpsilon)
                return move.TargetState == this.initialState;
            return false;
        }

        internal void RenameInitialState(int p)
        {
            List<Move<TConstraint>> moveList = this.delta[this.initialState];
            if (!this.delta.ContainsKey(p))
                this.delta[p] = new List<Move<TConstraint>>();
            if (!this.deltaInv.ContainsKey(p))
                this.deltaInv[p] = new List<Move<TConstraint>>();
            foreach (Move<TConstraint> move1 in moveList)
            {
                Move<TConstraint> move2 = Move<TConstraint>.To(p, move1.TargetState, move1.Condition);
                this.deltaInv[move1.TargetState].Remove(move1);
                this.deltaInv[move1.TargetState].Add(move2);
                this.delta[p].Add(move2);
            }
            if (this.finalStateSet.Contains(this.initialState))
            {
                this.finalStateSet.Remove(this.initialState);
                this.finalStateSet.Add(p);
            }
            this.delta.Remove(this.initialState);
            this.deltaInv.Remove(this.initialState);
            this.initialState = p;
        }

        internal void AddNewInitialStateThatIsFinal(int newInitialState)
        {
            this.finalStateSet.Add(newInitialState);
            List<Move<TConstraint>> moveList = new List<Move<TConstraint>>();
            moveList.Add(Move<TConstraint>.Epsilon(newInitialState, this.initialState));
            this.delta[newInitialState] = moveList;
            this.deltaInv[newInitialState] = new List<Move<TConstraint>>();
            this.deltaInv[this.initialState].Add(moveList[0]);
            this.isDeterministic = false;
            this.isEpsilonFree = false;
            this.maxState = Math.Max(this.maxState, newInitialState);
            this.initialState = newInitialState;
        }

        public SymbolicFiniteAutomaton<TConstraint> MakeCopy(int newInitialState)
        {
            int num = Math.Max(this.maxState, newInitialState) + 1;
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            dictionary[this.initialState] = newInitialState;
            List<Move<TConstraint>> moveList = new List<Move<TConstraint>>();
            HashSet<int> intSet = new HashSet<int>();
            foreach (Move<TConstraint> move in this.GetMoves())
            {
                int sourceState;
                if (!dictionary.TryGetValue(move.SourceState, out sourceState))
                {
                    sourceState = num++;
                    dictionary[move.SourceState] = sourceState;
                    if (this.finalStateSet.Contains(move.SourceState))
                        intSet.Add(sourceState);
                }
                int targetState;
                if (!dictionary.TryGetValue(move.TargetState, out targetState))
                {
                    targetState = num++;
                    dictionary[move.TargetState] = targetState;
                    if (this.finalStateSet.Contains(move.TargetState))
                        intSet.Add(targetState);
                }
                moveList.Add(Move<TConstraint>.To(sourceState, targetState, move.Condition));
            }
            if (this.finalStateSet.Contains(this.initialState))
                intSet.Add(newInitialState);
            return SymbolicFiniteAutomaton<TConstraint>.Create(newInitialState, (IEnumerable<int>)intSet, (IEnumerable<Move<TConstraint>>)moveList);
        }

        public void RemoveState(int state)
        {
            foreach (Move<TConstraint> move in this.delta[state])
                this.deltaInv[move.TargetState].Remove(move);
            foreach (Move<TConstraint> move in this.deltaInv[state])
                this.delta[move.SourceState].Remove(move);
            this.finalStateSet.Remove(state);
        }

        internal void RemoveTheMove(Move<TConstraint> move)
        {
            this.delta[move.SourceState].Remove(move);
            this.deltaInv[move.TargetState].Remove(move);
        }

        internal TConstraint GetCondition(int source, int target)
        {
            foreach (Move<TConstraint> move in this.delta[source])
            {
                if (move.TargetState == target)
                    return move.Condition;
            }
            throw new RexException("Internal error");
        }

        public static SymbolicFiniteAutomaton<TConstraint> MkProduct(SymbolicFiniteAutomaton<TConstraint> a, SymbolicFiniteAutomaton<TConstraint> b, Func<TConstraint, TConstraint, TConstraint> conj, Func<TConstraint, TConstraint, TConstraint> disj, Func<TConstraint, bool> isSat)
        {
            a = a.RemoveEpsilons(disj);
            b = b.RemoveEpsilons(disj);
            Dictionary<Pair<int, int>, int> dictionary1 = new Dictionary<Pair<int, int>, int>();
            Pair<int, int> index1 = new Pair<int, int>(a.InitialState, b.InitialState);
            Stack<Pair<int, int>> pairStack = new Stack<Pair<int, int>>();
            pairStack.Push(index1);
            dictionary1[index1] = 0;
            Dictionary<int, List<Move<TConstraint>>> delta = new Dictionary<int, List<Move<TConstraint>>>();
            delta[0] = new List<Move<TConstraint>>();
            List<int> intList1 = new List<int>();
            intList1.Add(0);
            List<int> intList2 = new List<int>();
            if (a.IsFinalState(a.InitialState) && b.IsFinalState(b.InitialState))
                intList2.Add(0);
            int num = 1;
            while (pairStack.Count > 0)
            {
                Pair<int, int> index2 = pairStack.Pop();
                int sourceState = dictionary1[index2];
                List<Move<TConstraint>> moveList = delta[sourceState];
                foreach (Move<TConstraint> move1 in a.GetMovesFrom(index2.First))
                {
                    foreach (Move<TConstraint> move2 in b.GetMovesFrom(index2.Second))
                    {
                        TConstraint condition = conj(move1.Condition, move2.Condition);
                        if (isSat(condition))
                        {
                            Pair<int, int> key = new Pair<int, int>(move1.TargetState, move2.TargetState);
                            int targetState;
                            if (!dictionary1.TryGetValue(key, out targetState))
                            {
                                targetState = num;
                                ++num;
                                dictionary1[key] = targetState;
                                intList1.Add(targetState);
                                delta[targetState] = new List<Move<TConstraint>>();
                                pairStack.Push(key);
                                if (a.IsFinalState(move1.TargetState) && b.IsFinalState(move2.TargetState))
                                    intList2.Add(targetState);
                            }
                            moveList.Add(Move<TConstraint>.To(sourceState, targetState, condition));
                        }
                    }
                }
            }
            Dictionary<int, List<Move<TConstraint>>> dictionary2 = new Dictionary<int, List<Move<TConstraint>>>();
            foreach (int index2 in intList1)
                dictionary2[index2] = new List<Move<TConstraint>>();
            foreach (int index2 in intList1)
            {
                foreach (Move<TConstraint> move in delta[index2])
                    dictionary2[move.TargetState].Add(move);
            }
            Stack<int> intStack = new Stack<int>((IEnumerable<int>)intList2);
            HashSet<int> intSet = new HashSet<int>((IEnumerable<int>)intList2);
            while (intStack.Count > 0)
            {
                foreach (Move<TConstraint> move in dictionary2[intStack.Pop()])
                {
                    if (!intSet.Contains(move.SourceState))
                    {
                        intStack.Push(move.SourceState);
                        intSet.Add(move.SourceState);
                    }
                }
            }
            if (intSet.Count == 0)
                return SymbolicFiniteAutomaton<TConstraint>.Empty;
            List<int> intList3 = new List<int>();
            foreach (int key in intList1)
            {
                if (!intSet.Contains(key))
                    delta.Remove(key);
                else
                    intList3.Add(key);
            }
            List<int> intList4 = intList3;
            foreach (int index2 in intList4)
            {
                List<Move<TConstraint>> moveList = new List<Move<TConstraint>>();
                foreach (Move<TConstraint> move in delta[index2])
                {
                    if (intSet.Contains(move.TargetState))
                        moveList.Add(move);
                }
                delta[index2] = moveList;
            }
            if (intList4.Count == 0)
                return SymbolicFiniteAutomaton<TConstraint>.Empty;
            SymbolicFiniteAutomaton<TConstraint> sfa = SymbolicFiniteAutomaton<TConstraint>.Create(0, (IEnumerable<int>)intList2, SymbolicFiniteAutomaton<TConstraint>.EnumerateMoves(delta));
            sfa.isEpsilonFree = true;
            sfa.isDeterministic = a.IsDeterministic || b.IsDeterministic;
            return sfa;
        }

        private static IEnumerable<Move<TConstraint>> EnumerateMoves(Dictionary<int, List<Move<TConstraint>>> delta)
        {
            foreach (KeyValuePair<int, List<Move<TConstraint>>> keyValuePair in delta)
            {
                foreach (Move<TConstraint> move in keyValuePair.Value)
                    yield return move;
            }
        }

        public SymbolicFiniteAutomaton<TConstraint> RemoveEpsilonLoops(Func<TConstraint, TConstraint, TConstraint> disj)
        {
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            foreach (int state in this.States)
            {
                IntSet intSet = new IntSet(this.GetEpsilonClosure(state));
                dictionary[state] = intSet.Intersect(this.GetInvEpsilonClosure(state)).Choice;
            }
            Dictionary<Pair<int, int>, TConstraint> conditionMap = new Dictionary<Pair<int, int>, TConstraint>();
            HashSet<Move<TConstraint>> eMoves = new HashSet<Move<TConstraint>>();
            foreach (Move<TConstraint> move in this.GetMoves())
            {
                int num1 = dictionary[move.SourceState];
                int num2 = dictionary[move.TargetState];
                if (move.IsEpsilon)
                {
                    if (num1 != num2)
                        eMoves.Add(Move<TConstraint>.Epsilon(num1, num2));
                }
                else
                {
                    Pair<int, int> key = new Pair<int, int>(num1, num2);
                    TConstraint s;
                    conditionMap[key] = !conditionMap.TryGetValue(key, out s) ? move.Condition : disj(s, move.Condition);
                }
            }
            int initialState = dictionary[this.InitialState];
            HashSet<int> intSet1 = new HashSet<int>();
            foreach (int finalState in this.GetFinalStates())
                intSet1.Add(dictionary[finalState]);
            return SymbolicFiniteAutomaton<TConstraint>.Create(initialState, (IEnumerable<int>)intSet1, this.EnumerateMoves(conditionMap, eMoves));
        }

        private IEnumerable<Move<TConstraint>> EnumerateMoves(Dictionary<Pair<int, int>, TConstraint> conditionMap, HashSet<Move<TConstraint>> eMoves)
        {
            foreach (KeyValuePair<Pair<int, int>, TConstraint> condition in conditionMap)
                yield return Move<TConstraint>.To(condition.Key.First, condition.Key.Second, condition.Value);
            foreach (Move<TConstraint> eMove in eMoves)
                yield return eMove;
        }

        public SymbolicFiniteAutomaton<TConstraint> RemoveEpsilons(Func<TConstraint, TConstraint, TConstraint> disj)
        {
            SymbolicFiniteAutomaton<TConstraint> sfa1 = this;
            if (sfa1.IsEpsilonFree)
                return sfa1;
            Dictionary<Pair<int, int>, TConstraint> dictionary = new Dictionary<Pair<int, int>, TConstraint>();
            foreach (Move<TConstraint> move in sfa1.GetMoves())
            {
                if (!move.IsEpsilon)
                {
                    Pair<int, int> key = new Pair<int, int>(move.SourceState, move.TargetState);
                    TConstraint s;
                    dictionary[key] = !dictionary.TryGetValue(key, out s) ? move.Condition : disj(move.Condition, s);
                }
            }
            foreach (int state in sfa1.States)
            {
                foreach (int sourceState in sfa1.GetEpsilonClosure(state))
                {
                    if (sourceState != state)
                    {
                        foreach (Move<TConstraint> move in sfa1.GetMovesFrom(sourceState))
                        {
                            if (!move.IsEpsilon)
                            {
                                Pair<int, int> key = new Pair<int, int>(state, move.TargetState);
                                TConstraint s;
                                dictionary[key] = !dictionary.TryGetValue(key, out s) || s.Equals((object)move.Condition) ? move.Condition : disj(move.Condition, s);
                            }
                        }
                    }
                }
            }
            Dictionary<int, List<Move<TConstraint>>> delta = new Dictionary<int, List<Move<TConstraint>>>();
            foreach (int state in sfa1.States)
                delta[state] = new List<Move<TConstraint>>();
            foreach (KeyValuePair<Pair<int, int>, TConstraint> keyValuePair in dictionary)
                delta[keyValuePair.Key.First].Add(Move<TConstraint>.To(keyValuePair.Key.First, keyValuePair.Key.Second, keyValuePair.Value));
            Stack<int> intStack = new Stack<int>();
            intStack.Push(sfa1.InitialState);
            HashSet<int> intSet = new HashSet<int>();
            intSet.Add(sfa1.InitialState);
            while (intStack.Count > 0)
            {
                foreach (Move<TConstraint> move in delta[intStack.Pop()])
                {
                    if (!intSet.Contains(move.TargetState))
                    {
                        intStack.Push(move.TargetState);
                        intSet.Add(move.TargetState);
                    }
                }
            }
            List<int> intList1 = new List<int>();
            foreach (int state in sfa1.States)
            {
                if (intSet.Contains(state))
                    intList1.Add(state);
                else
                    delta.Remove(state);
            }
            List<int> intList2 = new List<int>();
            foreach (int state1 in intList1)
            {
                foreach (int state2 in sfa1.GetEpsilonClosure(state1))
                {
                    if (sfa1.IsFinalState(state2))
                    {
                        intList2.Add(state1);
                        break;
                    }
                }
            }
            SymbolicFiniteAutomaton<TConstraint> sfa2 = SymbolicFiniteAutomaton<TConstraint>.Create(sfa1.InitialState, (IEnumerable<int>)intList2, SymbolicFiniteAutomaton<TConstraint>.EnumerateMoves(delta));
            sfa2.isEpsilonFree = true;
            return sfa2;
        }
    }
}
