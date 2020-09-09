using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace SJP.GenerationRex.Tests
{
    [TestFixture]
    internal static class RoundTripTests
    {
        [Test]
        public static void GenerateMembers_WithDigitsForAsciiEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            var engine = new RexEngine(Encoding.ASCII, seed);
            var result = engine.GenerateMembers(regex, 1).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
        }

        [Test]
        public static void GenerateMembers_WithDigitsForAsciiEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.ASCII, seed);
            var results = engine.GenerateMembers(regex, suiteSize);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
                }
            });
        }

        [Test]
        public static void GenerateMembers_WithDigitsForUnicodeEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            var engine = new RexEngine(Encoding.Unicode, seed);
            var result = engine.GenerateMembers(regex, 1).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
        }

        [Test]
        public static void GenerateMembers_WithDigitsForUnicodeEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\d\d\d - \d\d\d\d$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.Unicode, seed);
            var results = engine.GenerateMembers(regex, suiteSize);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
                }
            });
        }

        [Test]
        public static void GenerateMembers_WithWhiteSpaceAndDigitsForAsciiEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            var engine = new RexEngine(Encoding.ASCII, seed);
            var result = engine.GenerateMembers(regex, 1).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
        }

        [Test]
        public static void GenerateMembers_WithWhiteSpaceAndDigitsForAsciiEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.ASCII, seed);
            var results = engine.GenerateMembers(regex, suiteSize);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex);
                    Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
                }
            });
        }

        [Test]
        public static void GenerateMembers_WithWhiteSpaceAndDigitsForUnicodeEncoding_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            var engine = new RexEngine(Encoding.Unicode, seed);
            var result = engine.GenerateMembers(regex, 1).Single();

            var matches = Regex.IsMatch(result, regex);

            Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
        }

        [Test]
        public static void GenerateMembers_WithWhiteSpaceAndDigitsForUnicodeEncodingWithLargeSuite_ExecutesRoundTrip()
        {
            const int seed = 123;
            const string regex = @"^\s{3}\d{4} -\s+\d{3}$";
            const int suiteSize = 100;
            var engine = new RexEngine(Encoding.Unicode, seed);
            var results = engine.GenerateMembers(regex, suiteSize);

            Assert.Multiple(() =>
            {
                foreach (var result in results)
                {
                    var matches = Regex.IsMatch(result, regex, RegexOptions.CultureInvariant);
                    Assert.That(matches, Is.True, $"Generated result '{ result }' does not match regular expression '{ regex }'");
                }
            });
        }
    }
}
