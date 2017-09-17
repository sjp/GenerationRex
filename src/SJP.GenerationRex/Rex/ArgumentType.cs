using System;

namespace SJP.GenerationRex
{
    [Flags]
    internal enum ArgumentType
    {
        Required = 1,
        Unique = 2,
        Multiple = 4,
        AtMostOnce = 0,
        LastOccurenceWins = Multiple,
        MultipleUnique = LastOccurenceWins | Unique,
        AtLeastOnce = LastOccurenceWins | Required,
    }
}
