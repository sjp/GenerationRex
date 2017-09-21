using System.Text;
using System.Text.RegularExpressions;

namespace SJP.GenerationRex
{
    public sealed class RexSettings
    {
        public string[] regexes;
        //[Argument(ArgumentType.AtMostOnce, HelpText = "File where input regexes are stored one regex per line. This argument must be given if no regexes are given explicitly.", ShortName = "r")]
        public string regexfile;
        //[Argument(ArgumentType.Multiple, HelpText = "Zero or more regular expression options", ShortName = "o")]
        public RegexOptions[] options;
        //[Argument(ArgumentType.AtMostOnce, DefaultValue = 1, HelpText = "Number of members to generate", ShortName = "k")]
        public int k;
        //[Argument(ArgumentType.AtMostOnce, HelpText = "File where the generated strings are stored, if omitted, the output it directed to the console", ShortName = "f")]
        public string file;
        public Encoding encoding;
        //[Argument(ArgumentType.AtMostOnce, DefaultValue = -1, HelpText = "Random seed for the generation, -1 means that no seed is specified", ShortName = "s")]
        public int seed;
        //[Argument(ArgumentType.AtMostOnce, HelpText = "Name of output dot file of the finite automaton for the regex(es)", ShortName = "d")]
        public string dot;
        //[Argument(ArgumentType.AtMostOnce, DefaultValue = false, HelpText = "If set, intersect the regexes; otherwise treat the regexes independently and generate k members for each", ShortName = "i")]
        public bool intersect;

        public RexSettings(params string[] regexes)
        {
            if (regexes.Length < 1)
                throw new RexException("At least one regex must be specified");
            this.regexes = regexes;
            this.options = (RegexOptions[])null;
            this.k = 1;
            this.file = (string)null;
            this.encoding = Encoding.Unicode;
            this.seed = -1;
        }
    }
}
