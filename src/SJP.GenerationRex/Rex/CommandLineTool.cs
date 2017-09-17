using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SJP.GenerationRex
{
    internal static class CommandLineTool
    {
        public static void Run(string[] args)
        {
            RexSettings rexSettings = new RexSettings();
            //if (!CommandLineParser.ParseArgumentsWithUsage(args, (object) rexSettings))
            //  return;
            //TextWriter textWriter = rexSettings.file == null || rexSettings.file == "" ? Console.Out : (TextWriter) new StreamWriter(rexSettings.file);
            RexEngine rexEngine = new RexEngine(rexSettings.encoding, rexSettings.seed);
            RegexOptions options = RegexOptions.None;
            if (rexSettings.options != null)
            {
                foreach (RegexOptions option in rexSettings.options)
                    options |= option;
            }
            List<string> stringList = new List<string>() { @"\d\d\d\d-\d\d\d\d" };
            //if (rexSettings.regexes != null)
            //  stringList.AddRange((IEnumerable<string>) rexSettings.regexes);
            //if (rexSettings.regexfile != null)
            //  stringList.AddRange((IEnumerable<string>) File.ReadAllLines(rexSettings.regexfile));
            //if (stringList.Count == 0)
            //  throw new RexException("No regexes are given.");
            List<SymbolicFiniteAutomaton<BinaryDecisionDiagram>> sfaList = new List<SymbolicFiniteAutomaton<BinaryDecisionDiagram>>();
            if (rexSettings.intersect)
            {
                SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfaFromRegexes = rexEngine.CreateSFAFromRegexes(options, stringList.ToArray());
                sfaList.Add(sfaFromRegexes);
            }
            else
            {
                foreach (string str in stringList)
                {
                    SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfaFromRegexes = rexEngine.CreateSFAFromRegexes(options, str);
                    sfaList.Add(sfaFromRegexes);
                }
            }
            if (rexSettings.dot != null && rexSettings.dot != "")
            {
                TextWriter dot = (TextWriter)new StreamWriter(rexSettings.dot);
                rexEngine.ToDot(dot, sfaList[0]);
                dot.Close();
            }
            foreach (SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa in sfaList)
            {
                foreach (string member in rexEngine.GenerateMembers(sfa, rexSettings.k))
                {
                    string str = RexEngine.Escape(member);
                    //textWriter.WriteLine(str);
                }
            }
            //textWriter.Close();
        }
    }
}
