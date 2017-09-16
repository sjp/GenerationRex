using System;

namespace Rex
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                    args = new string[1] { "/?" };
                CommandLineTool.Run(args);
                return 0;
            }
            catch (RexException ex)
            {
                Console.Error.WriteLine(string.Format("Rex error: {0}", ex.Message));
                return -1;
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine(string.Format("Regex parsing error: {0}", ex.Message));
                return -1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format("Unexpected error: {0}", ex.ToString()));
                return -1;
            }
        }
    }
}
