using System;

namespace SJP.GenerationRex
{
    public class RexException : Exception
    {
        public RexException(string message)
          : base(message)
        {
        }

        internal const string NotSupported = "The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution";
        internal const string MisplacedEndAnchor = "The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop";
        internal const string MisplacedStartAnchor = "The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop";
        internal const string UnrecognizedRegex = "Unrecognized regex construct";
        internal const string InvalidFinalStates = "The set of final states must be a subset of all states";
        internal const string NoFinalState = "There is no final state";
        internal const string InternalError = "Internal error";
    }
}
