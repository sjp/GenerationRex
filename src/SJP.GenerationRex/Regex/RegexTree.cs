using System;
using System.Text.RegularExpressions;

namespace SJP.GenerationRex.RegularExpressions;

internal sealed class RegexTree
{
    internal RegexTree(RegexNode root, RegexOptions options)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
        Options = options;
    }

    public RegexNode Root { get; }

    public RegexOptions Options { get; }
}
