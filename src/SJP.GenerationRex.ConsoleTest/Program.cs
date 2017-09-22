using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SJP.GenerationRex.ConsoleTest
{
    internal static class Program
    {
        private static void Main()
        {
            var engine = new RexEngine();
            const string str = @"^\d{5}-\d\d\d\d$";

            var testValues = new[] { @"^\d\d\d\w\w$", @"^\w\w\d\d\d$" };

            var results = engine.GenerateMembers(@"^\d$", 1000);
            var resultList = results.ToList();
            var x = 1;
            x++;
        }
    }
}
