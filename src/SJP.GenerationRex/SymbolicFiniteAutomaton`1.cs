using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.GenerationRex
{
    internal sealed class SymbolicFiniteAutomaton<TConstraint>
    {
        public static SymbolicFiniteAutomaton<TConstraint> Empty = Create(0, Array.Empty<int>(), Array.Empty<Move<TConstraint>>());
        public static SymbolicFiniteAutomaton<TConstraint> Epsilon = Create(0, new int[1], Array.Empty<Move<TConstraint>>());
        private Dictionary<int, IList<Move<TConstraint>>> _delta; // lookup for state to outbound states (i.e. those that are pointed to by the key)
        private Dictionary<int, IList<Move<TConstraint>>> _deltaInv; // lookup for state to inbound states (i.e. those that point to the key)
        private int _initialState;
        private ISet<int> _finalStateSet;
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
                throw new RexException(RexException.NoFinalState);
            }
        }

        public int OutDegree(int state) => _delta[state].Count;

        public bool InitialStateIsSource => _deltaInv[_initialState].Count == 0;

        public bool IsEpsilonFree => _isEpsilonFree;

        public bool IsDeterministic => _isDeterministic;

        public IEnumerable<int> GetEpsilonClosure(int state)
        {
            var done = new HashSet<int> { state };
            var stack = new Stack<int>();
            stack.Push(state);

            while (stack.Count > 0)
            {
                var s = stack.Pop();
                yield return s;

                var epsilonMoves = _delta[s].Where(m => m.IsEpsilon && !done.Contains(m.TargetState));
                foreach (var move in epsilonMoves)
                {
                    done.Add(move.TargetState);
                    stack.Push(move.TargetState);
                }
            }
        }

        public IEnumerable<int> GetInvEpsilonClosure(int state)
        {
            var stack = new Stack<int>();
            stack.Push(state);

            var done = new HashSet<int> { state };
            while (stack.Count > 0)
            {
                var s = stack.Pop();
                yield return s;

                var epsilonMoves = _deltaInv[s].Where(m => m.IsEpsilon && !done.Contains(m.SourceState));
                foreach (var move in epsilonMoves)
                {
                    done.Add(move.SourceState);
                    stack.Push(move.SourceState);
                }
            }
        }

        public bool IsEmpty => _finalStateSet.Count == 0;

        public static SymbolicFiniteAutomaton<TConstraint> Create(int initialState, IEnumerable<int> finalStates, IEnumerable<Move<TConstraint>> moves)
        {
            var delta = new Dictionary<int, IList<Move<TConstraint>>>();
            var deltaInv = new Dictionary<int, IList<Move<TConstraint>>>();
            delta[initialState] = new List<Move<TConstraint>>();
            deltaInv[initialState] = new List<Move<TConstraint>>();
            bool isEpsilonFree = true;
            int maxState = initialState;
            bool isDeterministic = true;
            foreach (var move in moves)
            {
                if (move.IsEpsilon)
                    isEpsilonFree = false;
                if (!delta.ContainsKey(move.SourceState))
                    delta[move.SourceState] = new List<Move<TConstraint>>();
                if (!delta.ContainsKey(move.TargetState))
                    delta[move.TargetState] = new List<Move<TConstraint>>();
                if (!deltaInv.ContainsKey(move.SourceState))
                    deltaInv[move.SourceState] = new List<Move<TConstraint>>();
                if (!deltaInv.ContainsKey(move.TargetState))
                    deltaInv[move.TargetState] = new List<Move<TConstraint>>();
                delta[move.SourceState].Add(move);
                deltaInv[move.TargetState].Add(move);
                isDeterministic = isDeterministic && delta[move.SourceState].Count < 2;
                maxState = Math.Max(maxState, Math.Max(move.SourceState, move.TargetState));
            }
            var intSet = new HashSet<int>(finalStates);
            if (!intSet.IsSubsetOf(delta.Keys))
                throw new RexException(RexException.InvalidFinalStates);
            return new SymbolicFiniteAutomaton<TConstraint>
            {
                _initialState = initialState,
                _finalStateSet = intSet,
                _isEpsilonFree = isEpsilonFree,
                _maxState = maxState,
                _delta = delta,
                _deltaInv = deltaInv,
                _isDeterministic = isDeterministic
            };
        }

        private SymbolicFiniteAutomaton()
        {
        }

        public int InitialState => _initialState;

        public int MaxState => _maxState;

        public IEnumerable<int> States => _delta.Keys;

        public IEnumerable<Move<TConstraint>> GetMoves() => States.SelectMany(s => _delta[s]);

        public IEnumerable<int> GetFinalStates() => _finalStateSet;

        public IEnumerable<Move<TConstraint>> GetMovesFrom(int sourceState) => _delta[sourceState];

        public int GetMovesCountFrom(int sourceState) => _delta[sourceState].Count;

        public Move<TConstraint> GetNthMoveFrom(int sourceState, int n) => _delta[sourceState][n];

        public bool IsFinalState(int state) => _finalStateSet.Contains(state);

        public bool HasSingleFinalSink => _finalStateSet.Count == 1 && _delta[FinalState].Count == 0;

        public void Concat(SymbolicFiniteAutomaton<TConstraint> fa)
        {
            foreach (int state in fa.States)
            {
                _delta[state] = new List<Move<TConstraint>>(fa._delta[state]);
                _deltaInv[state] = new List<Move<TConstraint>>(fa._deltaInv[state]);
            }
            if (HasSingleFinalSink)
            {
                foreach (int finalState in _finalStateSet)
                {
                    foreach (Move<TConstraint> move1 in _deltaInv[finalState])
                    {
                        _delta[move1.SourceState].Remove(move1);
                        Move<TConstraint> move2 = Move<TConstraint>.To(move1.SourceState == finalState ? fa.InitialState : move1.SourceState, fa.InitialState, move1.Condition);
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
                    Move<TConstraint> move = Move<TConstraint>.Epsilon(finalState, fa._initialState);
                    _delta[finalState].Add(move);
                    _deltaInv[fa._initialState].Add(move);
                }
                _isEpsilonFree = false;
                _isDeterministic = false;
            }
            _finalStateSet = fa._finalStateSet;
            _maxState = Math.Max(_maxState, fa._maxState);
        }

        internal void MakeInitialStateFinal()
        {
            _finalStateSet.Add(_initialState);
        }

        internal void AddMove(Move<TConstraint> move)
        {
            if (!_delta.ContainsKey(move.SourceState))
                _delta[move.SourceState] = new List<Move<TConstraint>>();
            if (!_deltaInv.ContainsKey(move.TargetState))
                _deltaInv[move.TargetState] = new List<Move<TConstraint>>();
            _delta[move.SourceState].Add(move);
            _deltaInv[move.TargetState].Add(move);
            _maxState = Math.Max(_maxState, Math.Max(move.SourceState, move.TargetState));
            _isEpsilonFree = _isEpsilonFree && !move.IsEpsilon;
            _isDeterministic = false;
        }

        public bool IsKleeneClosure()
        {
            return IsFinalState(_initialState)
                && !_finalStateSet.Any(f => f != _initialState && !_delta[f].Any(IsEpsilonMoveToInitialState));
        }

        internal bool IsEpsilonMoveToInitialState(Move<TConstraint> move) => move.IsEpsilon && move.TargetState == _initialState;

        internal void RenameInitialState(int p)
        {
            var moveList = _delta[_initialState];
            if (!_delta.ContainsKey(p))
                _delta[p] = new List<Move<TConstraint>>();
            if (!_deltaInv.ContainsKey(p))
                _deltaInv[p] = new List<Move<TConstraint>>();
            foreach (var move1 in moveList)
            {
                Move<TConstraint> move2 = Move<TConstraint>.To(p, move1.TargetState, move1.Condition);
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
            var moveList = new List<Move<TConstraint>>
            {
                Move<TConstraint>.Epsilon(newInitialState, _initialState)
            };
            _delta[newInitialState] = moveList;
            _deltaInv[newInitialState] = new List<Move<TConstraint>>();
            _deltaInv[_initialState].Add(moveList[0]);
            _isDeterministic = false;
            _isEpsilonFree = false;
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

        public static SymbolicFiniteAutomaton<TConstraint> MkProduct(SymbolicFiniteAutomaton<TConstraint> sfa1, SymbolicFiniteAutomaton<TConstraint> sfa2, Func<TConstraint, TConstraint, TConstraint> conjunction, Func<TConstraint, TConstraint, TConstraint> disjunction, Func<TConstraint, bool> isSat)
        {
            sfa1 = sfa1.RemoveEpsilons(disjunction);
            sfa2 = sfa2.RemoveEpsilons(disjunction);
            var dictionary1 = new Dictionary<Pair<int, int>, int>();
            var index1 = new Pair<int, int>(sfa1.InitialState, sfa2.InitialState);
            var pairStack = new Stack<Pair<int, int>>();
            pairStack.Push(index1);
            dictionary1[index1] = 0;
            var delta = new Dictionary<int, List<Move<TConstraint>>> { [0] = new List<Move<TConstraint>>() };
            var intList1 = new List<int> { 0 };
            var intList2 = new List<int>();
            if (sfa1.IsFinalState(sfa1.InitialState) && sfa2.IsFinalState(sfa2.InitialState))
                intList2.Add(0);
            int state = 1;
            while (pairStack.Count > 0)
            {
                var index2 = pairStack.Pop();
                var sourceState = dictionary1[index2];
                var moveList = delta[sourceState];
                foreach (var move1 in sfa1.GetMovesFrom(index2.First))
                {
                    foreach (var move2 in sfa2.GetMovesFrom(index2.Second))
                    {
                        var condition = conjunction(move1.Condition, move2.Condition);
                        if (isSat(condition))
                        {
                            var key = new Pair<int, int>(move1.TargetState, move2.TargetState);
                            if (!dictionary1.TryGetValue(key, out var targetState))
                            {
                                targetState = state;
                                ++state;
                                dictionary1[key] = targetState;
                                intList1.Add(targetState);
                                delta[targetState] = new List<Move<TConstraint>>();
                                pairStack.Push(key);
                                if (sfa1.IsFinalState(move1.TargetState) && sfa2.IsFinalState(move2.TargetState))
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
                foreach (var move in delta[index2])
                    dictionary2[move.TargetState].Add(move);
            }
            var intStack = new Stack<int>(intList2);
            var intSet = new HashSet<int>(intList2);
            while (intStack.Count > 0)
            {
                foreach (var move in dictionary2[intStack.Pop()])
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
            var intList4 = intList3;
            foreach (int index2 in intList4)
            {
                var moveList = new List<Move<TConstraint>>();
                foreach (var move in delta[index2])
                {
                    if (intSet.Contains(move.TargetState))
                        moveList.Add(move);
                }
                delta[index2] = moveList;
            }
            if (intList4.Count == 0)
                return Empty;
            var sfa = Create(0, intList2, EnumerateMoves(delta));
            sfa._isEpsilonFree = true;
            sfa._isDeterministic = sfa1.IsDeterministic || sfa2.IsDeterministic;
            return sfa;
        }

        private static IEnumerable<Move<TConstraint>> EnumerateMoves(Dictionary<int, List<Move<TConstraint>>> delta) => delta.SelectMany(kv => kv.Value);

        public SymbolicFiniteAutomaton<TConstraint> RemoveEpsilonLoops(Func<TConstraint, TConstraint, TConstraint> disj)
        {
            var dictionary = new Dictionary<int, int>();
            foreach (int state in States)
            {
                var intSet = new IntSet(GetEpsilonClosure(state));
                dictionary[state] = intSet.Intersect(GetInvEpsilonClosure(state)).Choice;
            }
            var conditionMap = new Dictionary<Pair<int, int>, TConstraint>();
            var epsilonMoves = new HashSet<Move<TConstraint>>();
            foreach (var move in GetMoves())
            {
                int num1 = dictionary[move.SourceState];
                int num2 = dictionary[move.TargetState];
                if (move.IsEpsilon)
                {
                    if (num1 != num2)
                        epsilonMoves.Add(Move<TConstraint>.Epsilon(num1, num2));
                }
                else
                {
                    var key = new Pair<int, int>(num1, num2);
                    conditionMap[key] = !conditionMap.TryGetValue(key, out var s) ? move.Condition : disj(s, move.Condition);
                }
            }
            int initialState = dictionary[InitialState];
            var finalStates = new HashSet<int>();
            foreach (var finalState in GetFinalStates())
                finalStates.Add(dictionary[finalState]);
            return Create(initialState, finalStates, EnumerateMoves(conditionMap, epsilonMoves));
        }

        private static IEnumerable<Move<TConstraint>> EnumerateMoves(Dictionary<Pair<int, int>, TConstraint> conditionMap, HashSet<Move<TConstraint>> epsilonMoves)
        {
            var conditionMoves = conditionMap.Select(condition => Move<TConstraint>.To(condition.Key.First, condition.Key.Second, condition.Value));
            return conditionMoves.Concat(epsilonMoves);
        }

        public SymbolicFiniteAutomaton<TConstraint> RemoveEpsilons(Func<TConstraint, TConstraint, TConstraint> disj)
        {
            if (IsEpsilonFree)
                return this;

            var dictionary = new Dictionary<Pair<int, int>, TConstraint>();
            var nonEpsilonMoves = GetMoves().Where(m => !m.IsEpsilon);
            foreach (var move in nonEpsilonMoves)
            {
                var key = new Pair<int, int>(move.SourceState, move.TargetState);
                dictionary[key] = !dictionary.TryGetValue(key, out var s) ? move.Condition : disj(move.Condition, s);
            }

            foreach (int state in States)
            {
                var nonEpsMoves = GetEpsilonClosure(state)
                    .Where(sourceState => state != sourceState)
                    .SelectMany(GetMovesFrom)
                    .Where(m => !m.IsEpsilon);

                foreach (var move in nonEpsMoves)
                {
                    var key = new Pair<int, int>(state, move.TargetState);
                    dictionary[key] = !dictionary.TryGetValue(key, out var s) || s.Equals(move.Condition) ? move.Condition : disj(move.Condition, s);
                }
            }
            var delta = new Dictionary<int, List<Move<TConstraint>>>();
            foreach (int state in States)
                delta[state] = new List<Move<TConstraint>>();
            foreach (var kv in dictionary)
                delta[kv.Key.First].Add(Move<TConstraint>.To(kv.Key.First, kv.Key.Second, kv.Value));
            var intStack = new Stack<int>();
            intStack.Push(InitialState);
            var intSet = new HashSet<int> { InitialState };
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
            foreach (int state in States)
            {
                if (intSet.Contains(state))
                    intList1.Add(state);
                else
                    delta.Remove(state);
            }
            var intList2 = new List<int>();
            foreach (int state1 in intList1)
            {
                foreach (int state2 in GetEpsilonClosure(state1))
                {
                    if (IsFinalState(state2))
                    {
                        intList2.Add(state1);
                        break;
                    }
                }
            }
            var result = Create(InitialState, intList2, EnumerateMoves(delta));
            result._isEpsilonFree = true;
            return result;
        }
    }
}
