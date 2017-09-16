namespace System.Text.RegularExpressions
{
    internal sealed class RegexPrefix
    {
        internal RegexPrefix(string prefix, bool ignoreCase)
        {
            Prefix = prefix;
            CaseInsensitive = ignoreCase;
        }

        internal string Prefix { get; }

        internal bool CaseInsensitive { get; }

        internal static RegexPrefix Empty { get; } = new RegexPrefix(string.Empty, false);
    }
}
