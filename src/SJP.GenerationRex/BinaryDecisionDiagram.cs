namespace SJP.GenerationRex;

/// <summary>
/// Represents a binary decision diagram
/// </summary>
internal class BinaryDecisionDiagram
{
    public BinaryDecisionDiagram(int id, int ordinalValue)
    {
        Id = id;
        Ordinal = ordinalValue;
    }

    public BinaryDecisionDiagram(int id, int ordinalValue, BinaryDecisionDiagram trueCase, BinaryDecisionDiagram falseCase)
    {
        Id = id;
        Ordinal = ordinalValue;
        TrueCase = trueCase;
        FalseCase = falseCase;
    }

    public int Id { get; }

    public int Ordinal { get; }

    public static BinaryDecisionDiagram True { get; } = new BinaryDecisionDiagram(1, short.MaxValue);

    public static BinaryDecisionDiagram False { get; } = new BinaryDecisionDiagram(0, short.MaxValue);

    public BinaryDecisionDiagram TrueCase { get; set; }

    public BinaryDecisionDiagram FalseCase { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is BinaryDecisionDiagram bdd)
            return bdd.Id == Id;

        return false;
    }

    public override int GetHashCode() => Id;
}
