﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EnumsNET;

namespace SJP.GenerationRex
{
    internal class UnicodeCategoryRangeGenerator
    {
        public UnicodeCategoryRangeGenerator(Encoding encoding, int bits)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _bits = bits;

            _categoryLoader = new Lazy<IReadOnlyDictionary<UnicodeCategory, BinaryDecisionDiagram>>(GenerateCategory);
            _whitespaceLoader = new Lazy<BinaryDecisionDiagram>(GenerateWhiteSpace);
            _wordCharacterLoader = new Lazy<BinaryDecisionDiagram>(GenerateWordCharacter);
            _rangesLoader = new Lazy<Tuple<IReadOnlyDictionary<UnicodeCategory, Ranges>, Ranges>>(GenerateRanges);
        }

        public IReadOnlyDictionary<UnicodeCategory, BinaryDecisionDiagram> Category => _categoryLoader.Value;

        public BinaryDecisionDiagram WhiteSpace => _whitespaceLoader.Value;

        public BinaryDecisionDiagram WordCharacter => _wordCharacterLoader.Value;

        private IReadOnlyDictionary<UnicodeCategory, BinaryDecisionDiagram> GenerateCategory()
        {
            var bddBuilder = new BddBuilder(_bits);
            var result = new Dictionary<UnicodeCategory, BinaryDecisionDiagram>();
            var sourceRanges = _rangesLoader.Value.Item1;
            foreach (var kv in sourceRanges)
                result[kv.Key] = bddBuilder.MkBddForIntRanges(kv.Value.ranges);

            return result;
        }

        private BinaryDecisionDiagram GenerateWhiteSpace()
        {
            var bddBuilder = new BddBuilder(_bits);
            var ranges = _rangesLoader.Value.Item2.ranges;
            return bddBuilder.MkBddForIntRanges(ranges);
        }

        private BinaryDecisionDiagram GenerateWordCharacter()
        {
            var categories = Category;
            var bddBuilder = new BddBuilder(_bits);
            var wordCharacterBdd = bddBuilder.MkOr(
                categories[UnicodeCategory.UppercaseLetter],
                bddBuilder.MkOr(
                    categories[UnicodeCategory.LowercaseLetter],
                    bddBuilder.MkOr(
                        categories[UnicodeCategory.TitlecaseLetter],
                        bddBuilder.MkOr(
                            categories[UnicodeCategory.ModifierLetter],
                            bddBuilder.MkOr(
                                categories[UnicodeCategory.OtherLetter],
                                bddBuilder.MkOr(
                                    categories[UnicodeCategory.DecimalDigitNumber],
                                    categories[UnicodeCategory.ConnectorPunctuation]))))));

            return wordCharacterBdd;
        }

        private Tuple<IReadOnlyDictionary<UnicodeCategory, Ranges>, Ranges> GenerateRanges()
        {
            var categoryRange = new Dictionary<UnicodeCategory, Ranges>();
            foreach (var category in Enums.GetValues<UnicodeCategory>())
                categoryRange[category] = new Ranges();

            const char questionMark = '?';
            const int questionMarkIndex = questionMark;
            var whitepaceRanges = new Ranges();
            var unicode = Encoding.Unicode;
            var bitMaxValue = (1 << _bits) - 1;
            for (var n = 0; n <= bitMaxValue; n++)
            {
                var sourceC = (char)n;
                var bytes = unicode.GetBytes(new[] { sourceC });
                var convertedBytes = Encoding.Convert(unicode, _encoding, bytes);
                var str = _encoding.GetString(convertedBytes);

                UnicodeCategory category;
                if (string.IsNullOrWhiteSpace(str))
                {
                    category = UnicodeCategory.OtherNotAssigned;
                }
                else
                {
                    var c = str[0];
                    if (char.IsWhiteSpace(c))
                        whitepaceRanges.Add(n);
                    category = c == questionMark && n != questionMarkIndex
                        ? UnicodeCategory.OtherNotAssigned
                        : char.GetUnicodeCategory(c);
                }

                categoryRange[category].Add(n);
            }

            return new Tuple<IReadOnlyDictionary<UnicodeCategory, Ranges>, Ranges>(categoryRange, whitepaceRanges);
        }

        private readonly Lazy<IReadOnlyDictionary<UnicodeCategory, BinaryDecisionDiagram>> _categoryLoader;
        private readonly Lazy<BinaryDecisionDiagram> _whitespaceLoader;
        private readonly Lazy<BinaryDecisionDiagram> _wordCharacterLoader;
        private readonly Lazy<Tuple<IReadOnlyDictionary<UnicodeCategory, Ranges>, Ranges>> _rangesLoader;
        private readonly Encoding _encoding;
        private readonly int _bits;

        private class Ranges
        {
            internal List<int[]> ranges = new List<int[]>();

            internal void Add(int n)
            {
                for (var index = 0; index < ranges.Count; ++index)
                {
                    if (ranges[index][1] == n - 1)
                    {
                        ranges[index][1] = n;
                        return;
                    }
                }
                ranges.Add(new[] { n, n });
            }

            internal int Count => ranges.Count;
        }
    }
}
