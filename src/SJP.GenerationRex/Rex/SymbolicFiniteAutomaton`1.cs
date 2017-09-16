using System;
using System.Collections.Generic;
using System.Linq;

namespace Rex
{
    internal class SymbolicFiniteAutomaton<S>
    {
        public static SymbolicFiniteAutomaton<S> Empty = Create(0, new int[0], new Move<S>[0]);
        public static SymbolicFiniteAutomaton<S> Epsilon = Create(0, new int[1], new Move<S>[0]);
        private Dictionary<int, IList<Move<S>>> _delta;
        private Dictionary<int, IList<Move<S>>> _deltaInv;
        private int _initialState;
        private HashSet<int> _finalStateSet;
        private int _maxState;
        internal bool _isEpsilonFree;
        internal bool _isDeterministic;

        public int FinalState
        {
            get
            {
                using (var enumerator = _finalStateSet.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        return enumerator.Current;
                }

                throw new RexException("There is no final state");
            }
        }

        public bool HasMoreThanOneFinalState => _finalStateSet.Count > 1;

        public int OutDegree(int state) => _delta[state].Count;

        public Move<S> GetMoveFrom(int state)
        {
            return _delta[state][0];
        }

        public bool InitialStateIsSource => _deltaInv[_initialState].Count == 0;

        public bool IsEpsilonFree => _isEpsilonFree;

        public int MoveCount => _delta.Values.Sum(v => v.Count);

        public bool IsDeterministic => _isDeterministic;

        public IEnumerable<int> GetEpsilonClosure(int state)
        {
            var stack = new Stack<int>();
            var done = new HashSet<int>();
            done.Add(state);
            stack.Push(state);
            while (stack.Count > 0)
            {
                int s = stack.Pop();
                yield return s;
                foreach (Move<S> move in _delta[s])
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
            var stack = new Stack<int>();
            var done = new HashSet<int>();
            done.Add(state);
            stack.Push(state);
            while (stack.Count > 0)
            {
                int s = stack.Pop();
                yield return s;
                foreach (Move<S> move in _deltaInv[s])
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
                return _finalStateSet.Count == 0;
            }
        }

        public static SymbolicFiniteAutomaton<S> Create(int initialState, IEnumerable<int> finalStates, IEnumerable<Move<S>> moves)
        {
            var dictionary1 = new Dictionary<int, IList<Move<S>>>();
            var dictionary2 = new Dictionary<int, IList<Move<S>>>();
            dictionary1[initialState] = new List<Move<S>>();
            dictionary2[initialState] = new List<Move<S>>();
            bool flag1 = true;
            int val1 = initialState;
            bool flag2 = true;
            foreach (Move<S> move in moves)
            {
                if (move.IsEpsilon)
                    flag1 = false;
                if (!dictionary1.ContainsKey(move.SourceState))
                    dictionary1[move.SourceState] = new List<Move<S>>();
                if (!dictionary1.ContainsKey(move.TargetState))
                    dictionary1[move.TargetState] = new List<Move<S>>();
                if (!dictionary2.ContainsKey(move.SourceState))
                    dictionary2[move.SourceState] = new List<Move<S>>();
                if (!dictionary2.ContainsKey(move.TargetState))
                    dictionary2[move.TargetState] = new List<Move<S>>();
                dictionary1[move.SourceState].Add(move);
                dictionary2[move.TargetState].Add(move);
                flag2 = flag2 && dictionary1[move.SourceState].Count < 2;
                val1 = Math.Max(val1, Math.Max(move.SourceState, move.TargetState));
            }
            var intSet = new HashSet<int>(finalStates);
            if (!intSet.IsSubsetOf(dictionary1.Keys))
                throw new RexException("The set of final states must be a subset of all states");
            return new SymbolicFiniteAutomaton<S>() { _initialState = initialState, _finalStateSet = intSet, _isEpsilonFree = flag1, _maxState = val1, _delta = dictionary1, _deltaInv = dictionary2, _isDeterministic = flag2 };
        }

        private SymbolicFiniteAutomaton()
        {
        }

        public int InitialState
        {
            get
            {
                return _initialState;
            }
        }

        public int MaxState => _maxState;

        public IEnumerable<int> States => _delta.Keys;

        public int StateCount
        {
            get
            {
                return _delta.Count;
            }
        }

        public IEnumerable<Move<S>> GetMoves()
        {
            foreach (int state in States)
            {
                foreach (Move<S> move in _delta[state])
                    yield return move;
            }
        }

        public IEnumerable<Move<S>> GetEpsilonMoves()
        {
            foreach (int state in States)
            {
                foreach (Move<S> move in _delta[state])
                {
                    if (move.IsEpsilon)
                        yield return move;
                }
            }
        }

        public IEnumerable<int> GetEpsilonTargetsFrom(int state)
        {
            foreach (Move<S> move in _delta[state])
            {
                if (move.IsEpsilon)
                    yield return move.TargetState;
            }
        }

        public IEnumerable<int> GetFinalStates()
        {
            return _finalStateSet;
        }

        public IEnumerable<Move<S>> GetMovesFrom(int sourceState)
        {
            return _delta[sourceState];
        }

        public int GetMovesCountFrom(int sourceState)
        {
            return _delta[sourceState].Count;
        }

        public Move<S> GetNthMoveFrom(int sourceState, int n)
        {
            return _delta[sourceState][n];
        }

        public IEnumerable<Move<S>> GetMovesFromStates(IEnumerable<int> sourceStates)
        {
            foreach (int sourceState in sourceStates)
            {
                foreach (Move<S> move in _delta[sourceState])
                    yield return move;
            }
        }

        public bool IsFinalState(int state)
        {
            return _finalStateSet.Contains(state);
        }

        public bool HasSingleFinalSink
        {
            get
            {
                if (_finalStateSet.Count == 1)
                    return _delta[FinalState].Count == 0;
                return false;
            }
        }

        public void Concat(SymbolicFiniteAutomaton<S> fa)
        {
            foreach (int state in fa.States)
            {
                _delta[state] = new List<Move<S>>(fa._delta[state]);
                _deltaInv[state] = new List<Move<S>>(fa._deltaInv[state]);
            }
            if (HasSingleFinalSink)
            {
                foreach (int finalState in _finalStateSet)
                {
                    foreach (Move<S> move1 in _deltaInv[finalState])
                    {
                        _delta[move1.SourceState].Remove(move1);
                        Move<S> move2 = Move<S>.T(move1.SourceState == finalState ? fa.InitialState : move1.SourceState, fa.InitialState, move1.Condition);
                        _delta[move2.SourceState].Add(move2);
                        _deltaInv[move2.TargetState].Add(move2);
                    }
                    _delta.Remove(finalState);
                    _deltaInv.Remove(finalState);
                }
                if (_finalStateSet.Contains(_initialState))
                    _initialState = fa._initialState;
                _isEpsilonFree = _isEpsilonFree && fa._isEpsilonFree;
                _isDeterministic = _isDeterministic && fa._isDeterministic;
            }
            else
            {
                foreach (int finalState in _finalStateSet)
                {
                    Move<S> move = Move<S>.Epsilon(finalState, fa._initialState);
                    _delta[finalState].Add(move);
                    _deltaInv[fa._initialState].Add(move);
                }
                _isEpsilonFree = false;
                _isDeterministic = false;
            }
            _finalStateSet = fa._finalStateSet;
            _maxState = Math.Max(_maxState, fa._maxState);
        }

        private bool AllFinalStatesAreSinks
        {
            get
            {
                foreach (int finalState in _finalStateSet)
                {
                    if (_delta[finalState].Count > 0)
                        return false;
                }
                return true;
            }
        }

        internal void SetFinalStates(IEnumerable<int> newFinalStates)
        {
            _finalStateSet = new HashSet<int>(newFinalStates);
        }

        internal void MakeInitialStateFinal()
        {
            _finalStateSet.Add(_initialState);
        }

        internal void AddMove(Move<S> move)
        {
            if (!_delta.ContainsKey(move.SourceState))
                _delta[move.SourceState] = new List<Move<S>>();
            if (!_deltaInv.ContainsKey(move.TargetState))
                _deltaInv[move.TargetState] = new List<Move<S>>();
            _delta[move.SourceState].Add(move);
            _deltaInv[move.TargetState].Add(move);
            _maxState = Math.Max(_maxState, Math.Max(move.SourceState, move.TargetState));
            _isEpsilonFree = _isEpsilonFree && !move.IsEpsilon;
            _isDeterministic = false;
        }

        public bool IsKleeneClosure()
        {
            if (!IsFinalState(_initialState))
                return false;
            foreach (int finalState in _finalStateSet)
            {
                if (finalState != _initialState && !_delta[finalState].Any(IsEpsilonMoveToInitialState))
                    return false;
            }
            return true;
        }

        internal bool IsEpsilonMoveToInitialState(Move<S> move)
        {
            if (move.IsEpsilon)
                return move.TargetState == _initialState;
            return false;
        }

        internal void RenameInitialState(int p)
        {
            IList<Move<S>> moveList = _delta[_initialState];
            if (!_delta.ContainsKey(p))
                _delta[p] = new List<Move<S>>();
            if (!_deltaInv.ContainsKey(p))
                _deltaInv[p] = new List<Move<S>>();
            foreach (Move<S> move1 in moveList)
            {
                Move<S> move2 = Move<S>.T(p, move1.TargetState, move1.Condition);
                _deltaInv[move1.TargetState].Remove(move1);
                _deltaInv[move1.TargetState].Add(move2);
                _delta[p].Add(move2);
            }
            if (_finalStateSet.Contains(_initialState))
            {
                _finalStateSet.Remove(_initialState);
                _finalStateSet.Add(p);
            }
            _delta.Remove(_initialState);
            _deltaInv.Remove(_initialState);
            _initialState = p;
        }

        internal void AddNewInitialStateThatIsFinal(int newInitialState)
        {
            _finalStateSet.Add(newInitialState);
            var moveList = new List<Move<S>>();
            moveList.Add(Move<S>.Epsilon(newInitialState, _initialState));
            _delta[newInitialState] = moveList;
            _deltaInv[newInitialState] = new List<Move<S>>();
            _deltaInv[_initialState].Add(moveList[0]);
            _isDeterministic = false;
            _isEpsilonFree = false;
            _maxState = Math.Max(_maxState, newInitialState);
            _initialState = newInitialState;
        }

        public SymbolicFiniteAutomaton<S> MakeCopy(int newInitialState)
        {
            int num = Math.Max(_maxState, newInitialState) + 1;
            var dictionary = new Dictionary<int, int>();
            dictionary[_initialState] = newInitialState;
            var moveList = new List<Move<S>>();
            var intSet = new HashSet<int>();
            foreach (Move<S> move in GetMoves())
            {
                int sourceState;
                if (!dictionary.TryGetValue(move.SourceState, out sourceState))
                {
                    sourceState = num++;
                    dictionary[move.SourceState] = sourceState;
                    if (_finalStateSet.Contains(move.SourceState))
                        intSet.Add(sourceState);
                }
                int targetState;
                if (!dictionary.TryGetValue(move.TargetState, out targetState))
                {
                    targetState = num++;
                    dictionary[move.TargetState] = targetState;
                    if (_finalStateSet.Contains(move.TargetState))
                        intSet.Add(targetState);
                }
                moveList.Add(Move<S>.T(sourceState, targetState, move.Condition));
            }
            if (_finalStateSet.Contains(_initialState))
                intSet.Add(newInitialState);
            return SymbolicFiniteAutomaton<S>.Create(newInitialState, intSet, moveList);
        }

        public void RemoveState(int state)
        {
            foreach (Move<S> move in _delta[state])
                _deltaInv[move.TargetState].Remove(move);
            foreach (Move<S> move in _deltaInv[state])
                _delta[move.SourceState].Remove(move);
            _finalStateSet.Remove(state);
        }

        internal void RemoveTheMove(Move<S> move)
        {
            _delta[move.SourceState].Remove(move);
            _deltaInv[move.TargetState].Remove(move);
        }

        internal S GetCondition(int source, int target)
        {
            foreach (Move<S> move in _delta[source])
            {
                if (move.TargetState == target)
                    return move.Condition;
            }
            throw new RexException("Internal error");
        }

        public static SymbolicFiniteAutomaton<S> MkProduct(SymbolicFiniteAutomaton<S> a, SymbolicFiniteAutomaton<S> b, Func<S, S, S> conj, Func<S, S, S> disj, Func<S, bool> isSat)
        {
            a = a.RemoveEpsilons(disj);
            b = b.RemoveEpsilons(disj);
            var dictionary1 = new Dictionary<Pair<int, int>, int>();
            var index1 = new Pair<int, int>(a.InitialState, b.InitialState);
            var pairStack = new Stack<Pair<int, int>>();
            pairStack.Push(index1);
            dictionary1[index1] = 0;
            var delta = new Dictionary<int, List<Move<S>>>();
            delta[0] = new List<Move<S>>();
            var intList1 = new List<int>();
            intList1.Add(0);
            var intList2 = new List<int>();
            if (a.IsFinalState(a.InitialState) && b.IsFinalState(b.InitialState))
                intList2.Add(0);
            int num = 1;
            while (pairStack.Count > 0)
            {
                Pair<int, int> index2 = pairStack.Pop();
                int sourceState = dictionary1[index2];
                List<Move<S>> moveList = delta[sourceState];
                foreach (Move<S> move1 in a.GetMovesFrom(index2.First))
                {
                    foreach (Move<S> move2 in b.GetMovesFrom(index2.Second))
                    {
                        S condition = conj(move1.Condition, move2.Condition);
                        if (isSat(condition))
                        {
                            var key = new Pair<int, int>(move1.TargetState, move2.TargetState);
                            int targetState;
                            if (!dictionary1.TryGetValue(key, out targetState))
                            {
                                targetState = num;
                                ++num;
                                dictionary1[key] = targetState;
                                intList1.Add(targetState);
                                delta[targetState] = new List<Move<S>>();
                                pairStack.Push(key);
                                if (a.IsFinalState(move1.TargetState) && b.IsFinalState(move2.TargetState))
                                    intList2.Add(targetState);
                            }
                            moveList.Add(Move<S>.T(sourceState, targetState, condition));
                        }
                    }
                }
            }
            var dictionary2 = new Dictionary<int, List<Move<S>>>();
            foreach (int index2 in intList1)
                dictionary2[index2] = new List<Move<S>>();
            foreach (int index2 in intList1)
            {
                foreach (Move<S> move in delta[index2])
                    dictionary2[move.TargetState].Add(move);
            }
            var intStack = new Stack<int>(intList2);
            var intSet = new HashSet<int>(intList2);
            while (intStack.Count > 0)
            {
                foreach (Move<S> move in dictionary2[intStack.Pop()])
                {
                    if (!intSet.Contains(move.SourceState))
                    {
                        intStack.Push(move.SourceState);
                        intSet.Add(move.SourceState);
                    }
                }
            }
            if (intSet.Count == 0)
                return SymbolicFiniteAutomaton<S>.Empty;
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
                var moveList = new List<Move<S>>();
                foreach (Move<S> move in delta[index2])
                {
                    if (intSet.Contains(move.TargetState))
                        moveList.Add(move);
                }
                delta[index2] = moveList;
            }
            if (intList4.Count == 0)
                return SymbolicFiniteAutomaton<S>.Empty;
            SymbolicFiniteAutomaton<S> sfa = SymbolicFiniteAutomaton<S>.Create(0, intList2, SymbolicFiniteAutomaton<S>.EnumerateMoves(delta));
            sfa._isEpsilonFree = true;
            sfa._isDeterministic = a.IsDeterministic || b.IsDeterministic;
            return sfa;
        }

        private static IEnumerable<Move<S>> EnumerateMoves(Dictionary<int, List<Move<S>>> delta)
        {
            foreach (KeyValuePair<int, List<Move<S>>> keyValuePair in delta)
            {
                foreach (Move<S> move in keyValuePair.Value)
                    yield return move;
            }
        }

        public SymbolicFiniteAutomaton<S> RemoveEpsilonLoops(Func<S, S, S> disj)
        {
            var dictionary = new Dictionary<int, int>();
            foreach (int state in States)
            {
                var intSet = new IntSet(GetEpsilonClosure(state));
                dictionary[state] = intSet.Intersect(GetInvEpsilonClosure(state)).Choice;
            }
            var conditionMap = new Dictionary<Pair<int, int>, S>();
            var eMoves = new HashSet<Move<S>>();
            foreach (Move<S> move in GetMoves())
            {
                int num1 = dictionary[move.SourceState];
                int num2 = dictionary[move.TargetState];
                if (move.IsEpsilon)
                {
                    if (num1 != num2)
                        eMoves.Add(Move<S>.Epsilon(num1, num2));
                }
                else
                {
                    var key = new Pair<int, int>(num1, num2);
                    S s;
                    conditionMap[key] = !conditionMap.TryGetValue(key, out s) ? move.Condition : disj(s, move.Condition);
                }
            }
            int initialState = dictionary[InitialState];
            var intSet1 = new HashSet<int>();
            foreach (int finalState in GetFinalStates())
                intSet1.Add(dictionary[finalState]);
            return SymbolicFiniteAutomaton<S>.Create(initialState, intSet1, EnumerateMoves(conditionMap, eMoves));
        }

        private IEnumerable<Move<S>> EnumerateMoves(Dictionary<Pair<int, int>, S> conditionMap, HashSet<Move<S>> eMoves)
        {
            foreach (KeyValuePair<Pair<int, int>, S> condition in conditionMap)
                yield return Move<S>.T(condition.Key.First, condition.Key.Second, condition.Value);
            foreach (Move<S> eMove in eMoves)
                yield return eMove;
        }

        public SymbolicFiniteAutomaton<S> RemoveEpsilons(Func<S, S, S> disj)
        {
            var sfa1 = this;
            if (sfa1.IsEpsilonFree)
                return sfa1;
            var dictionary = new Dictionary<Pair<int, int>, S>();
            foreach (Move<S> move in sfa1.GetMoves())
            {
                if (!move.IsEpsilon)
                {
                    var key = new Pair<int, int>(move.SourceState, move.TargetState);
                    S s;
                    dictionary[key] = !dictionary.TryGetValue(key, out s) ? move.Condition : disj(move.Condition, s);
                }
            }
            foreach (int state in sfa1.States)
            {
                foreach (int sourceState in sfa1.GetEpsilonClosure(state))
                {
                    if (sourceState != state)
                    {
                        foreach (Move<S> move in sfa1.GetMovesFrom(sourceState))
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
            var delta = new Dictionary<int, List<Move<S>>>();
            foreach (int state in sfa1.States)
                delta[state] = new List<Move<S>>();
            foreach (KeyValuePair<Pair<int, int>, S> keyValuePair in dictionary)
                delta[keyValuePair.Key.First].Add(Move<S>.T(keyValuePair.Key.First, keyValuePair.Key.Second, keyValuePair.Value));
            var intStack = new Stack<int>();
            intStack.Push(sfa1.InitialState);
            var intSet = new HashSet<int>
            {
                sfa1.InitialState
            };
            while (intStack.Count > 0)
            {
                foreach (Move<S> move in delta[intStack.Pop()])
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
            SymbolicFiniteAutomaton<S> sfa2 = SymbolicFiniteAutomaton<S>.Create(sfa1.InitialState, intList2, SymbolicFiniteAutomaton<S>.EnumerateMoves(delta));
            sfa2._isEpsilonFree = true;
            return sfa2;
        }
    }
}
