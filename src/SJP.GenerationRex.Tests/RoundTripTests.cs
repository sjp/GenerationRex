using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace SJP.GenerationRex.Tests
{
    [TestFixture]
    public class RoundTripTests
    {
        [Test]
        public void GenerateMembers_WithDigitsForAsciiEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            var engine = new RexEngine(Encoding.ASCII, seed);
            var result = engine.GenerateMembers(new RegexOptions(), 1, regex).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
        }

        [Test]
        public void GenerateMembers_WithDigitsForAsciiEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.ASCII, seed);
            var results = engine.GenerateMembers(new RegexOptions(), suiteSize, regex);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
                }
            });
        }

        [Test]
        public void GenerateMembers_WithDigitsForCP437Encoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            var engine = new RexEngine(Encoding.ASCII, seed);
            var result = engine.GenerateMembers(new RegexOptions(), 1, regex).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
        }

        [Test]
        public void GenerateMembers_WithDigitsForCP437EncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.ASCII, seed);
            var results = engine.GenerateMembers(new RegexOptions(), suiteSize, regex);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
                }
            });
        }

        [Test]
        public void GenerateMembers_WithDigitsForUnicodeEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            var engine = new RexEngine(Encoding.Unicode, seed);
            var result = engine.GenerateMembers(new RegexOptions(), 1, regex).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
        }

        [Test]
        public void GenerateMembers_WithDigitsForUnicodeEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.Unicode, seed);
            var results = engine.GenerateMembers(new RegexOptions(), suiteSize, regex);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
                }
            });
        }

        [Test]
        public void GenerateMembers_WithWhiteSpaceAndDigitsForAsciiEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            var engine = new RexEngine(Encoding.ASCII, seed);
            var result = engine.GenerateMembers(new RegexOptions(), 1, regex).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
        }

        [Test]
        public void GenerateMembers_WithWhiteSpaceAndDigitsForAsciiEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.ASCII, seed);
            var results = engine.GenerateMembers(new RegexOptions(), suiteSize, regex);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
                }
            });
        }

        [Test]
        public void GenerateMembers_WithWhiteSpaceAndDigitsForCP437Encoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            var engine = new RexEngine(Encoding.ASCII, seed);
            var result = engine.GenerateMembers(new RegexOptions(), 1, regex).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
        }

        [Test]
        public void GenerateMembers_WithWhiteSpaceAndDigitsForCP437EncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.ASCII, seed);
            var results = engine.GenerateMembers(new RegexOptions(), suiteSize, regex);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
                }
            });
        }

        [Test]
        public void GenerateMembers_WithWhiteSpaceAndDigitsForUnicodeEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            var engine = new RexEngine(Encoding.Unicode, seed);
            var result = engine.GenerateMembers(new RegexOptions(), 1, regex).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
        }

        [Test]
        public void GenerateMembers_WithWhiteSpaceAndDigitsForUnicodeEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.Unicode, seed);
            var results = engine.GenerateMembers(new RegexOptions(), suiteSize, regex);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex, RegexOptions.CultureInvariant);
                    Assert.IsTrue(matches, $"Generated result '{ result }' does not match regular expresssion '{ regex }'");
                }
            });
        }
    }
}
