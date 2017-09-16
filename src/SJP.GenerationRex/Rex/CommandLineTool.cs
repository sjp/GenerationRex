using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Rex
{
    internal static class CommandLineTool
    {
        public static void Run(string[] args)
        {
            var rexSettings = new RexSettings();
            if (!CommandLineParser.ParseArgumentsWithUsage(args, rexSettings))
                return;
            TextWriter textWriter = rexSettings.file == null || rexSettings.file == "" ? Console.Out : new StreamWriter(rexSettings.file);
            var rexEngine = new RexEngine(rexSettings.encoding, rexSettings.seed);
            var options = RegexOptions.None;
            if (rexSettings.options != null)
            {
                foreach (RegexOptions option in rexSettings.options)
                    options |= option;
            }
            var stringList = new List<string>();
            if (rexSettings.regexes != null)
                stringList.AddRange(rexSettings.regexes);
            if (rexSettings.regexfile != null)
                stringList.AddRange(File.ReadAllLines(rexSettings.regexfile));
            if (stringList.Count == 0)
                throw new RexException("No regexes are given.");
            var sfaList = new List<SymbolicFiniteAutomaton<BinaryDecisionDiagram>>();
            if (rexSettings.intersect)
            {
                SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfaFromRegexes = rexEngine.CreateSfaFromRegexes(options, stringList.ToArray());
                sfaList.Add(sfaFromRegexes);
            }
            else
            {
                foreach (string str in stringList)
                {
                    SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfaFromRegexes = rexEngine.CreateSfaFromRegexes(options, str);
                    sfaList.Add(sfaFromRegexes);
                }
            }
            if (rexSettings.dot != null && rexSettings.dot != "")
            {
                TextWriter dot = new StreamWriter(rexSettings.dot);
                rexEngine.ToDot(dot, sfaList[0]);
                dot.Close();
            }
            foreach (SymbolicFiniteAutomaton<BinaryDecisionDiagram> sfa in sfaList)
            {
                foreach (string member in rexEngine.GenerateMembers(sfa, rexSettings.k))
                {
                    string str = RexEngine.Escape(member);
                    textWriter.WriteLine(str);
                }
            }
            textWriter.Close();
        }
    }
}
