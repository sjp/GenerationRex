﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SJP.GenerationRex
{
    internal static class UnicodeCategoryRangesGenerator
    {
        public static void Generate(string namespacename, string classname, string path)
        {
            if (classname == null)
                throw new ArgumentNullException(nameof(classname));
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path != "" && !path.EndsWith("/"))
                path += "/";
            string str1 = "//\r\n//Automatically generated by UnicodeCategoryRangesGenerator\r\n//\r\nnamespace " + namespacename + "\r\n{\r\npublic static class " + classname + "\r\n{";
            string str2 = "}\r\n}\r\n";
            FileInfo fileInfo = new FileInfo(string.Format("{1}{0}.cs", (object)classname, (object)path));
            if (fileInfo.Exists)
                fileInfo.IsReadOnly = false;
            StreamWriter sw = new StreamWriter(string.Format("{1}{0}.cs", (object)classname, (object)path));
            sw.WriteLine(str1);
            sw.WriteLine("#region ASCII");
            UnicodeCategoryRangesGenerator.WriteRangeFields(7, sw, "ASCII");
            sw.WriteLine("#endregion");
            sw.WriteLine();
            sw.WriteLine("#region CP437");
            UnicodeCategoryRangesGenerator.WriteRangeFields(8, sw, "CP437");
            sw.WriteLine("#endregion");
            sw.WriteLine();
            sw.WriteLine("#region Unicode (UTF16)");
            UnicodeCategoryRangesGenerator.WriteRangeFields(16, sw, "Unicode");
            sw.WriteLine("#endregion");
            sw.WriteLine();
            sw.WriteLine(str2);
            sw.Close();
        }

        private static void WriteRangeFields(int bits, StreamWriter sw, string field)
        {
            int num1 = (1 << bits) - 1;
            Dictionary<UnicodeCategory, UnicodeCategoryRangesGenerator.Ranges> dictionary = new Dictionary<UnicodeCategory, UnicodeCategoryRangesGenerator.Ranges>();
            for (int index = 0; index < 30; ++index)
                dictionary[(UnicodeCategory)index] = new UnicodeCategoryRangesGenerator.Ranges();
            UnicodeCategoryRangesGenerator.Ranges ranges = new UnicodeCategoryRangesGenerator.Ranges();
            for (int n = 0; n <= num1; ++n)
            {
                char c = (char)n;
                if (char.IsWhiteSpace(c))
                    ranges.Add(n);
                UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
                dictionary[unicodeCategory].Add(n);
            }
            BDD[] bddArray = new BDD[30];
            BddBuilder bddBuilder = new BddBuilder(bits);
            for (int index = 0; index < 30; ++index)
                bddArray[index] = bddBuilder.MkBddForIntRanges((IEnumerable<int[]>)dictionary[(UnicodeCategory)index].ranges);
            BDD bdd1 = bddBuilder.MkBddForIntRanges((IEnumerable<int[]>)ranges.ranges);
            BDD bdd2 = bddBuilder.MkOr(bddArray[0], bddBuilder.MkOr(bddArray[1], bddBuilder.MkOr(bddArray[2], bddBuilder.MkOr(bddArray[3], bddBuilder.MkOr(bddArray[4], bddBuilder.MkOr(bddArray[8], bddArray[18]))))));
            sw.WriteLine("/// <summary>\r\n/// Compact BDD encodings of the categories.\r\n/// </summary>");
            sw.WriteLine("public static int[][] " + field + "Bdd = new int[][]{");
            foreach (UnicodeCategory key in dictionary.Keys)
            {
                sw.WriteLine("//{0}({1}):", (object)key, (object)key);
                BDD bdd3 = bddArray[(int)key];
                if (bdd3 == null || bdd3 == BDD.False)
                    sw.WriteLine("null, //false");
                else if (bdd3 == BDD.True)
                {
                    sw.WriteLine("new int[]{0,0}, //true");
                }
                else
                {
                    sw.WriteLine("new int[]{");
                    foreach (int num2 in bddBuilder.SerializeCompact(bdd3))
                        sw.WriteLine("{0},", (object)num2);
                    sw.WriteLine("},");
                }
            }
            sw.WriteLine("};");
            sw.WriteLine("/// <summary>\r\n/// Compact BDD encoding of the whitespace characters.\r\n/// </summary>");
            sw.WriteLine("public static int[] " + field + "WhitespaceBdd = new int[]{");
            foreach (int num2 in bddBuilder.SerializeCompact(bdd1))
                sw.WriteLine("{0},", (object)num2);
            sw.WriteLine("};");
            sw.WriteLine("/// <summary>\r\n/// Compact BDD encoding of word characters is the BDD for the union of categories 0,1,2,3,4,8,18\r\n/// </summary>");
            sw.WriteLine("public static int[] " + field + "WordCharacterBdd = new int[]{");
            foreach (int num2 in bddBuilder.SerializeCompact(bdd2))
                sw.WriteLine("{0},", (object)num2);
            sw.WriteLine("};");
        }

        private class Ranges
        {
            internal List<int[]> ranges = new List<int[]>();

            internal Ranges()
            {
            }

            internal void Add(int n)
            {
                for (int index = 0; index < this.ranges.Count; ++index)
                {
                    if (this.ranges[index][1] == n - 1)
                    {
                        this.ranges[index][1] = n;
                        return;
                    }
                }
                this.ranges.Add(new int[2] { n, n });
            }

            internal int Count
            {
                get
                {
                    return this.ranges.Count;
                }
            }
        }
    }
}
