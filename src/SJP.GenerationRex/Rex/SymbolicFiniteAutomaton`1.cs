using System;
using System.Collections.Generic;

namespace SJP.GenerationRex
{
    internal class SymbolicFiniteAutomaton<TConstraint>
    {
        public static SymbolicFiniteAutomaton<TConstraint> Empty = Create(0, new int[0], new Move<TConstraint>[0]);
        public static SymbolicFiniteAutomaton<TConstraint> Epsilon = Create(0, new int[1], new Move<TConstraint>[0]);
        private Dictionary<int, List<Move<TConstraint>>> _delta;
        private Dictionary<int, List<Move<TConstraint>>> _deltaInv;
        private int _initialState;
        private HashSet<int> _finalStateSet;
        private int _maxState;
        internal bool isEpsilonFree;
        internal bool isDeterministic;

        public int FinalState
        {
            get
            {
                using (HashSet<int>.Enumerator enumerator = _finalStateSet.GetEnumerator())
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
                return _finalStateSet.Count > 1;
            }
        }

        public int OutDegree(int state)
        {
            return _delta[state].Count;
        }

        public Move<TConstraint> GetMoveFrom(int state)
        {
            return _delta[state][0];
        }

        public bool InitialStateIsSource
        {
            get
            {
                return _deltaInv[_initialState].Count == 0;
            }
        }

        public bool IsEpsilonFree
        {
            get
            {
                return isEpsilonFree;
            }
        }

        public int MoveCount
        {
            get
            {
                int num = 0;
                foreach (int key in _delta.Keys)
                    num += _delta[key].Count;
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
            var stack = new Stack<int>();
            var done = new HashSet<int> { state };
            stack.Push(state);
            while (stack.Count > 0)
            {
                int s = stack.Pop();
                yield return s;
                foreach (Move<TConstraint> move in this._delta[s])
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
                foreach (Move<TConstraint> move in this._deltaInv[s])
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
                return this._finalStateSet.Count == 0;
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
                _initialState = initialState,
                _finalStateSet = intSet,
                isEpsilonFree = flag1,
                _maxState = val1,
                _delta = dictionary1,
                _deltaInv = dictionary2,
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
                return this._initialState;
            }
        }

        public int MaxState
        {
            get
            {
                return this._maxState;
            }
        }

        public IEnumerable<int> States
        {
            get
            {
                return (IEnumerable<int>)this._delta.Keys;
            }
        }

        public int StateCount
        {
            get
            {
                return this._delta.Count;
            }
        }

        public IEnumerable<Move<TConstraint>> GetMoves()
        {
            foreach (int state in this.States)
            {
                foreach (Move<TConstraint> move in this._delta[state])
                    yield return move;
            }
        }

        public IEnumerable<Move<TConstraint>> GetEpsilonMoves()
        {
            foreach (int state in this.States)
            {
                foreach (Move<TConstraint> move in this._delta[state])
                {
                    if (move.IsEpsilon)
                        yield return move;
                }
            }
        }

        public IEnumerable<int> GetEpsilonTargetsFrom(int state)
        {
            foreach (Move<TConstraint> move in this._delta[state])
            {
                if (move.IsEpsilon)
                    yield return move.TargetState;
            }
        }

        public IEnumerable<int> GetFinalStates()
        {
            return (IEnumerable<int>)this._finalStateSet;
        }

        public IEnumerable<Move<TConstraint>> GetMovesFrom(int sourceState)
        {
            return (IEnumerable<Move<TConstraint>>)this._delta[sourceState];
        }

        public int GetMovesCountFrom(int sourceState)
        {
            return this._delta[sourceState].Count;
        }

        public Move<TConstraint> GetNthMoveFrom(int sourceState, int n)
        {
            return this._delta[sourceState][n];
        }

        public IEnumerable<Move<TConstraint>> GetMovesFromStates(IEnumerable<int> sourceStates)
        {
            foreach (int sourceState in sourceStates)
            {
                foreach (Move<TConstraint> move in this._delta[sourceState])
                    yield return move;
            }
        }

        public bool IsFinalState(int state)
        {
            return this._finalStateSet.Contains(state);
        }

        public bool HasSingleFinalSink
        {
            get
            {
                if (this._finalStateSet.Count == 1)
                    return this._delta[this.FinalState].Count == 0;
                return false;
            }
        }

        public void Concat(SymbolicFiniteAutomaton<TConstraint> fa)
        {
            foreach (int state in fa.States)
            {
                this._delta[state] = new List<Move<TConstraint>>((IEnumerable<Move<TConstraint>>)fa._delta[state]);
                this._deltaInv[state] = new List<Move<TConstraint>>((IEnumerable<Move<TConstraint>>)fa._deltaInv[state]);
            }
            if (this.HasSingleFinalSink)
            {
                foreach (int finalState in this._finalStateSet)
                {
                    foreach (Move<TConstraint> move1 in this._deltaInv[finalState])
                    {
                        this._delta[move1.SourceState].Remove(move1);
                        Move<TConstraint> move2 = Move<TConstraint>.To(move1.SourceState == finalState ? fa.InitialState : move1.SourceState, fa.InitialState, move1.Condition);
                        this._delta[move2.SourceState].Add(move2);
                        this._deltaInv[move2.TargetState].Add(move2);
                    }
                    this._delta.Remove(finalState);
                    this._deltaInv.Remove(finalState);
                }
                if (this._finalStateSet.Contains(this._initialState))
                    this._initialState = fa._initialState;
                this.isEpsilonFree = this.isEpsilonFree && fa.isEpsilonFree;
                this.isDeterministic = this.isDeterministic && fa.isDeterministic;
            }
            else
            {
                foreach (int finalState in this._finalStateSet)
                {
                    Move<TConstraint> move = Move<TConstraint>.Epsilon(finalState, fa._initialState);
                    this._delta[finalState].Add(move);
                    this._deltaInv[fa._initialState].Add(move);
                }
                this.isEpsilonFree = false;
                this.isDeterministic = false;
            }
            this._finalStateSet = fa._finalStateSet;
            this._maxState = Math.Max(this._maxState, fa._maxState);
        }

        private bool AllFinalStatesAreSinks
        {
            get
            {
                foreach (int finalState in this._finalStateSet)
                {
                    if (this._delta[finalState].Count > 0)
                        return false;
                }
                return true;
            }
        }

        internal void SetFinalStates(IEnumerable<int> newFinalStates)
        {
            this._finalStateSet = new HashSet<int>(newFinalStates);
        }

        internal void MakeInitialStateFinal()
        {
            this._finalStateSet.Add(this._initialState);
        }

        internal void AddMove(Move<TConstraint> move)
        {
            if (!this._delta.ContainsKey(move.SourceState))
                this._delta[move.SourceState] = new List<Move<TConstraint>>();
            if (!this._deltaInv.ContainsKey(move.TargetState))
                this._deltaInv[move.TargetState] = new List<Move<TConstraint>>();
            this._delta[move.SourceState].Add(move);
            this._deltaInv[move.TargetState].Add(move);
            this._maxState = Math.Max(this._maxState, Math.Max(move.SourceState, move.TargetState));
            this.isEpsilonFree = this.isEpsilonFree && !move.IsEpsilon;
            this.isDeterministic = false;
        }

        public bool IsKleeneClosure()
        {
            if (!this.IsFinalState(this._initialState))
                return false;
            foreach (int finalState in this._finalStateSet)
            {
                if (finalState != this._initialState && !this._delta[finalState].Exists(new Predicate<Move<TConstraint>>(this.IsEpsilonMoveToInitialState)))
                    return false;
            }
            return true;
        }

        internal bool IsEpsilonMoveToInitialState(Move<TConstraint> move)
        {
            if (move.IsEpsilon)
                return move.TargetState == this._initialState;
            return false;
        }

        internal void RenameInitialState(int p)
        {
            List<Move<TConstraint>> moveList = this._delta[this._initialState];
            if (!this._delta.ContainsKey(p))
                this._delta[p] = new List<Move<TConstraint>>();
            if (!this._deltaInv.ContainsKey(p))
                this._deltaInv[p] = new List<Move<TConstraint>>();
            foreach (Move<TConstraint> move1 in moveList)
            {
                Move<TConstraint> move2 = Move<TConstraint>.To(p, move1.TargetState, move1.Condition);
                this._deltaInv[move1.TargetState].Remove(move1);
                this._deltaInv[move1.TargetState].Add(move2);
                this._delta[p].Add(move2);
            }
            if (this._finalStateSet.Contains(this._initialState))
            {
                this._finalStateSet.Remove(this._initialState);
                this._finalStateSet.Add(p);
            }
            this._delta.Remove(this._initialState);
            this._deltaInv.Remove(this._initialState);
            this._initialState = p;
        }

        internal void AddNewInitialStateThatIsFinal(int newInitialState)
        {
            _finalStateSet.Add(newInitialState);
            var moveList = new List<Move<TConstraint>>
            {
                Move<TConstraint>.Epsilon(newInitialState, _initialState)
            };
            _delta[newInitialState] = moveList;
            _deltaInv[newInitialState] = new List<Move<TConstraint>>();
            _deltaInv[_initialState].Add(moveList[0]);
            isDeterministic = false;
            isEpsilonFree = false;
            _maxState = Math.Max(_maxState, newInitialState);
            _initialState = newInitialState;
        }

        public SymbolicFiniteAutomaton<TConstraint> MakeCopy(int newInitialState)
        {
            int num = Math.Max(_maxState, newInitialState) + 1;
            var dictionary = new Dictionary<int, int>
            {
                [_initialState] = newInitialState
            };
            var moveList = new List<Move<TConstraint>>();
            var intSet = new HashSet<int>();
            foreach (var move in GetMoves())
            {
                if (!dictionary.TryGetValue(move.SourceState, out var sourceState))
                {
                    sourceState = num++;
                    dictionary[move.SourceState] = sourceState;
                    if (_finalStateSet.Contains(move.SourceState))
                        intSet.Add(sourceState);
                }
                if (!dictionary.TryGetValue(move.TargetState, out var targetState))
                {
                    targetState = num++;
                    dictionary[move.TargetState] = targetState;
                    if (_finalStateSet.Contains(move.TargetState))
                        intSet.Add(targetState);
                }
                moveList.Add(Move<TConstraint>.To(sourceState, targetState, move.Condition));
            }
            if (_finalStateSet.Contains(_initialState))
                intSet.Add(newInitialState);
            return Create(newInitialState, intSet, moveList);
        }

        public void RemoveState(int state)
        {
            foreach (var move in _delta[state])
                _deltaInv[move.TargetState].Remove(move);
            foreach (var move in _deltaInv[state])
                _delta[move.SourceState].Remove(move);
            _finalStateSet.Remove(state);
        }

        internal void RemoveTheMove(Move<TConstraint> move)
        {
            _delta[move.SourceState].Remove(move);
            _deltaInv[move.TargetState].Remove(move);
        }

        internal TConstraint GetCondition(int source, int target)
        {
            foreach (var move in _delta[source])
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
            var dictionary1 = new Dictionary<Pair<int, int>, int>();
            var index1 = new Pair<int, int>(a.InitialState, b.InitialState);
            var pairStack = new Stack<Pair<int, int>>();
            pairStack.Push(index1);
            dictionary1[index1] = 0;
            var delta = new Dictionary<int, List<Move<TConstraint>>>
            {
                [0] = new List<Move<TConstraint>>()
            };
            var intList1 = new List<int> { 0 };
            var intList2 = new List<int>();
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
                            var key = new Pair<int, int>(move1.TargetState, move2.TargetState);
                            if (!dictionary1.TryGetValue(key, out var targetState))
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
            var dictionary2 = new Dictionary<int, List<Move<TConstraint>>>();
            foreach (int index2 in intList1)
                dictionary2[index2] = new List<Move<TConstraint>>();
            foreach (int index2 in intList1)
            {
                foreach (Move<TConstraint> move in delta[index2])
                    dictionary2[move.TargetState].Add(move);
            }
            var intStack = new Stack<int>(intList2);
            var intSet = new HashSet<int>(intList2);
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
                return Empty;
            var intList3 = new List<int>();
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
                var moveList = new List<Move<TConstraint>>();
                foreach (Move<TConstraint> move in delta[index2])
                {
                    if (intSet.Contains(move.TargetState))
                        moveList.Add(move);
                }
                delta[index2] = moveList;
            }
            if (intList4.Count == 0)
                return Empty;
            var sfa = Create(0, intList2, EnumerateMoves(delta));
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
            var dictionary = new Dictionary<int, int>();
            foreach (int state in this.States)
            {
                var intSet = new IntSet(GetEpsilonClosure(state));
                dictionary[state] = intSet.Intersect(GetInvEpsilonClosure(state)).Choice;
            }
            var conditionMap = new Dictionary<Pair<int, int>, TConstraint>();
            var eMoves = new HashSet<Move<TConstraint>>();
            foreach (var move in GetMoves())
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
                    var key = new Pair<int, int>(num1, num2);
                    conditionMap[key] = !conditionMap.TryGetValue(key, out var s) ? move.Condition : disj(s, move.Condition);
                }
            }
            int initialState = dictionary[InitialState];
            var intSet1 = new HashSet<int>();
            foreach (var finalState in GetFinalStates())
                intSet1.Add(dictionary[finalState]);
            return Create(initialState, intSet1, EnumerateMoves(conditionMap, eMoves));
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
            var sfa1 = this;
            if (sfa1.IsEpsilonFree)
                return sfa1;
            var dictionary = new Dictionary<Pair<int, int>, TConstraint>();
            foreach (Move<TConstraint> move in sfa1.GetMoves())
            {
                if (!move.IsEpsilon)
                {
                    var key = new Pair<int, int>(move.SourceState, move.TargetState);
                    dictionary[key] = !dictionary.TryGetValue(key, out var s) ? move.Condition : disj(move.Condition, s);
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
                                var key = new Pair<int, int>(state, move.TargetState);
                                dictionary[key] = !dictionary.TryGetValue(key, out var s) || s.Equals(move.Condition) ? move.Condition : disj(move.Condition, s);
                            }
                        }
                    }
                }
            }
            var delta = new Dictionary<int, List<Move<TConstraint>>>();
            foreach (int state in sfa1.States)
                delta[state] = new List<Move<TConstraint>>();
            foreach (var kv in dictionary)
                delta[kv.Key.First].Add(Move<TConstraint>.To(kv.Key.First, kv.Key.Second, kv.Value));
            var intStack = new Stack<int>();
            intStack.Push(sfa1.InitialState);
            var intSet = new HashSet<int> { sfa1.InitialState };
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
            var intList1 = new List<int>();
            foreach (int state in sfa1.States)
            {
                if (intSet.Contains(state))
                    intList1.Add(state);
                else
                    delta.Remove(state);
            }
            var intList2 = new List<int>();
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
            var sfa2 = Create(sfa1.InitialState, intList2, EnumerateMoves(delta));
            sfa2.isEpsilonFree = true;
            return sfa2;
        }
    }
}
