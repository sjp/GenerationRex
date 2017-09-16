using System;

namespace Rex
{
    [Flags]
    internal enum ArgumentType
    {
        AtMostOnce = 0,
        Required = 1,
        Unique = 2,
        Multiple = 4,
        LastOccurenceWins = Multiple,
        AtLeastOnce = LastOccurenceWins | Required,
        MultipleUnique = LastOccurenceWins | Unique
    }
}
