using NUnit.Framework;
using Moq;
using AlgorithmExtensions;

[TestFixture]
public class SubsetSumTests
{
    static IEnumerable<object[]> GetTestData()
    {
        yield return new object[] { new List<int> { 1,2,3,5 }, 4, new List<int> { 0,2 } };
        yield return new object[] { new List<int> { 1, 4, 9 }, 1,new List<int> { 0 } };
        yield return new object[] { new List<int> { 1,2,3,4,5,6,7,8,9,100},100, new List<int> { 9} };
    }


    [TestCaseSource(nameof(GetTestData))]
    public void FindMaxSubsetIndexes_WithMidrangeSum_ShouldReturnMatchValue(List<int> input, int N, List<int> expected)
    {
        // Arrange

        // Act 
        List<int> actual = SubsetSum.FindMaxSubsetIndexes(input, N);
        
        // Assert
        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    public void FindMaxSubsetIndexes_WithTooSmallSum_ShouldReturnEmptyList()
    {
        // Arrange
        List<int> numbers = Enumerable.Range(5, 5).ToList();
        int N = 3;
        List<int> expected = [];

        // Act 
        List<int> actual = SubsetSum.FindMaxSubsetIndexes(numbers, N);

        // Assert
        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    public void FindMaxSubsetIndexes_WithTooLargeSum_ShouldReturnWholeList()
    {
        // Arrange
        List<int> numbers = Enumerable.Range(1,5).ToList();
        int N = 30;
        List<int> expected = [0,1,2,3,4];

        // Act 
        List<int> actual = SubsetSum.FindMaxSubsetIndexes(numbers, N);

        // Assert
        Assert.That(actual, Is.EquivalentTo(expected));
    }
}
