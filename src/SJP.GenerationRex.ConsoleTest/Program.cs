using System;
using System.Linq;

namespace SJP.GenerationRex.ConsoleTest
{
    internal static class Program
    {
        private static void Main()
        {
            //const string path = @"C:\Users\sjp\Downloads";
            //UnicodeCategoryRangesGenerator.Generate("Rex", "UnicodeCategoryRanges", path);

            var engine = new RexEngine(CharacterEncoding.Unicode, 234);
            const string str = @"^\d\d\d\-\d\d\d\d$";
            var results = engine.GenerateMembers(new System.Text.RegularExpressions.RegexOptions(), 4, str);
            var resultList = results.ToList();
        }
    }
}
