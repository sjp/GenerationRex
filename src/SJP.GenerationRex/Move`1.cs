using System;

namespace SJP.GenerationRex;

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
        Condition = default;
    }

    public int SourceState { get; }

    public int TargetState { get; }

    public TCondition Condition { get; }

    public static Move<TCondition> To(int sourceState, int targetState, TCondition condition) => new(sourceState, targetState, condition);

    public static Move<TCondition> Epsilon(int sourceState, int targetState) => new(sourceState, targetState);

    public bool IsEpsilon => Condition == null;

    public bool Equals(Move<TCondition> other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (SourceState != other.SourceState || TargetState != other.TargetState)
            return false;

        if (Condition == null && other.Condition == null)
            return true;

        return other.Condition is object && other.Condition.Equals(Condition);
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return Equals(obj as Move<TCondition>);
    }

    public override int GetHashCode()
    {
        return SourceState + (TargetState * 2) + (Condition is null ? 0 : Condition.GetHashCode());
    }

    public override string ToString()
    {
        return "(" + SourceState + "," + (Condition is null ? "" : (object)(Condition + ",")) + TargetState + ")";
    }
}