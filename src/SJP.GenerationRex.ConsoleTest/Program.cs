using System.Linq;
using System.Text;

namespace SJP.GenerationRex.ConsoleTest
{
    internal static class Program
    {
        private static void Main()
        {
            //const string path = @"C:\Users\sjp\Downloads";
            //UnicodeCategoryRangesGenerator.Generate("SJP.GenerationRex", "UnicodeCategoryRanges", path);

            var engine = new RexEngine(Encoding.ASCII, 234);
            const string str = @"^\d{5}-\d\d\d\d$";
            var results = engine.GenerateMembers(new System.Text.RegularExpressions.RegexOptions(), 4, str);
            var resultList = results.ToList();
            var x = 1;
            x++;
        }
    }
}
