using System.Collections;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexTree
    {
        internal RegexNode _root;
        internal Hashtable _caps;
        internal int[] _capnumlist;
        internal Hashtable _capnames;
        internal string[] _capslist;
        internal RegexOptions _options;
        internal int _captop;

        internal RegexTree(RegexNode root, Hashtable caps, int[] capnumlist, int captop, Hashtable capnames, string[] capslist, RegexOptions opts)
        {
            _root = root;
            _caps = caps;
            _capnumlist = capnumlist;
            _capnames = capnames;
            _capslist = capslist;
            _captop = captop;
            _options = opts;
        }
    }
}
