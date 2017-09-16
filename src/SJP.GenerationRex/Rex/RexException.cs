using System;

namespace Rex
{
    public class RexException : Exception
    {
        public const string NotSupported = "The following constructs are currently not supported: anchors \\G, \\b, \\B, named groups, lookahead, lookbehind, as-few-times-as-possible quantifiers, backreferences, conditional alternation, substitution";
        public const string MissingDotViewer = "Dot viewer is not installed";
        public const string MisplacedEndAnchor = "The anchor \\z, \\Z or $ is not supported if it is followed by other regex patterns or is nested in a loop";
        public const string MisplacedStartAnchor = "The anchor \\A or ^ is not supported if it is preceded by other regex patterns or is nested in a loop";
        public const string UnrecognizedRegex = "Unrecognized regex construct";
        public const string FSAisNotClean = "FSA is not clean";
        public const string InvalidFinalStates = "The set of final states must be a subset of all states";
        public const string NoFinalState = "There is no final state";
        public const string InternalError = "Internal error";

        public RexException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        public RexException(string message)
          : base(message)
        {
        }
    }
}
