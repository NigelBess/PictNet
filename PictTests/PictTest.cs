using PictNet;
using PictTests.TestUtil;

namespace PictTests
{
    [TestClass]
    public sealed class PictTest
    {
        [TestMethod]
        public void GenerateIndices_1_3_3_4_MatchesExpectedSequence()
        {
            var counts = new[] { 1, 3, 3, 4 };

            var rows = Pict.GenerateIndices(counts, 2);

            List<List<int>> expected =
            [
                [0, 0, 1, 1],
                [0, 1, 0, 2],
                [0, 2, 0, 0],
                [0, 2, 2, 1],
                [0, 1, 1, 0],
                [0, 1, 0, 1],
                [0, 2, 1, 2],
                [0, 0, 1, 3],
                [0, 1, 0, 3],
                [0, 0, 2, 0],
                [0, 1, 2, 2],
                [0, 2, 2, 3],
                [0, 0, 0, 2]
            ];

            Assert.HasCount(expected.Count, rows, "Unexpected row count.");

            foreach (var (row, expectedRow) in rows.Zip(expected))
            {
                Assert.HasCount(counts.Length, row, "Unexpected column count in a row.");
                CollectionAssert.AreEqual(expectedRow, row);
            }
        }


        [TestMethod]
        [DynamicData(nameof(AllPairsCases), DynamicDataSourceType.Method)]
        public void TestAllPairsPresent(List<int> valueCounts, int order, int seed)
        {
            var rows = Pict.GenerateIndices(valueCounts, (uint)order);
            AssertAllPairsPresent(rows, valueCounts, order);
        }

        [TestMethod]
        public void TestCombinations()
        {
            List<IList<string>> parameterOptions = [
                ["A1"],
                ["B1", "B2", "B3"],
                ["C1", "C2", "C3"],
                ["D1","D2","D3","D4"]
            ];

            var combinations = Pict.GenerateCombinations(parameterOptions, 2);

        }

        public static IEnumerable<object[]> AllPairsCases()
        {
            yield return new object[] { new List<int> { 2, 2 }, 2, 1 };
            yield return new object[] { new List<int> { 2, 3 }, 3, 2 };
            yield return new object[] { new List<int> { 3, 3, 3 }, 2, 3 };
            yield return new object[] { new List<int> { 2, 2, 2, 2 }, 2, 0 };
            yield return new object[] { new List<int> { 2, 3, 4 }, 3, 0 };
        }


        private void AssertAllPairsPresent(List<List<int>> rows, IList<int> valueCounts, int order)
        {
            var combinations = CombinationGenerator.GenerateAllCombinations(valueCounts, order);
            foreach (var combo in combinations)
            {
                Assert.IsTrue(rows.Any(row => row.ContainsCombination(combo)));
            }
        }
    }
}
