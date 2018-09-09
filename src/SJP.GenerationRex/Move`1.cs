using System;

namespace SJP.GenerationRex
{
    internal sealed class Move<TCondition> : IEquatable<Move<TCondition>>
    {
        private Move(int sourceState, int targetState, TCondition condition)
        {
            SourceState = sourceState;
            TargetState = targetState;
            Condition = condition;
        }

        private Move(int sourceState, int targetState)
        {
            SourceState = sourceState;
            TargetState = targetState;
            Condition = default(TCondition);
        }

        public int SourceState { get; }

        public int TargetState { get; }

        public TCondition Condition { get; }

        public static Move<TCondition> To(int sourceState, int targetState, TCondition condition) => new Move<TCondition>(sourceState, targetState, condition);

        public static Move<TCondition> Epsilon(int sourceState, int targetState) => new Move<TCondition>(sourceState, targetState);

        public bool IsEpsilon => ReferenceEquals(Condition, null);

        public bool Equals(Move<TCondition> other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (SourceState != other.SourceState || TargetState != other.TargetState)
                return false;

            if (ReferenceEquals(Condition, null) && ReferenceEquals(other.Condition, null))
                return true;

            return !ReferenceEquals(other.Condition, null) && other.Condition.Equals(Condition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as Move<TCondition>);
        }

        public override int GetHashCode()
        {
            return SourceState + (TargetState * 2) + (ReferenceEquals(Condition, null) ? 0 : Condition.GetHashCode());
        }

        public override string ToString()
        {
            return "(" + SourceState + "," + (ReferenceEquals(Condition, null) ? "" : (object)(Condition + ",")) + TargetState + ")";
        }
    }
}
