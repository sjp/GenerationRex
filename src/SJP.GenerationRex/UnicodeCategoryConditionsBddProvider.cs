using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SJP.GenerationRex
{
    internal class UnicodeCategoryConditionsBddProvider : IUnicodeCategoryConditions<BinaryDecisionDiagram>
    {
        internal UnicodeCategoryConditionsBddProvider(Encoding encoding, int bits)
        {
            if (!_cache.TryGetValue(encoding.CodePage, out var builder))
            {
                builder = new UnicodeCategoryRangeGenerator(encoding, bits);
                _cache.TryAdd(encoding.CodePage, builder);
            }

            _catConditions = builder.Category;
            WhiteSpaceCondition = builder.WhiteSpace;
            WordLetterCondition = builder.WordCharacter;
        }

        public BinaryDecisionDiagram CategoryCondition(UnicodeCategory category) => _catConditions[category];

        public BinaryDecisionDiagram WhiteSpaceCondition { get; }

        public BinaryDecisionDiagram WordLetterCondition { get; }

        private readonly IDictionary<UnicodeCategory, BinaryDecisionDiagram> _catConditions;

        private static readonly ConcurrentDictionary<int, UnicodeCategoryRangeGenerator> _cache = new ConcurrentDictionary<int, UnicodeCategoryRangeGenerator>();
    }
}
