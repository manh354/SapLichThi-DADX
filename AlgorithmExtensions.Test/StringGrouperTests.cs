using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmExtensions.Test
{
    [TestFixture]
    public class StringGrouperTests
    {
        static IEnumerable<object[]> GetTestData()
        {
            yield return new object[] {
                new List<string> { "based","based a","based b","c"},
                new List<List<string>> {
                    new() { "based", "based a", "based b" },
                    new() { "c"} }
                };
            yield return new object[] {
                new List<string> { "XSTK","XSTK và quy hoạch thực nghiệm","XSTK", "Giải tích I", "Giải tích II", "Giải tích III"},
                new List<List<string>> {
                    new() {"XSTK", "XSTK","XSTK và quy hoạch thực nghiệm"},
                    new() { "Giải tích I", "Giải tích II", "Giải tích III" } }
                };
        }

        [TestCaseSource(nameof(GetTestData))]
        public void FindMaxSubsetIndexes_WithMidrangeSum_ShouldReturnMatchValue(IEnumerable<string> input, List<List<string>> expected)
        {
            // Arrange

            // Act 
            List<List<string>> actual = StringGrouper.GroupStringsStart(input);

            // Assert
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}
