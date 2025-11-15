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

            var order = 2;

            var rows = Pict.GenerateIndices(counts, (uint)order);

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

            AssertAllPairsPresent(rows, counts, order);
        }

        [TestMethod]
        public void AssertSeedAffectsOutcome()
        {
            var counts = new[] { 1, 3, 3, 4 };

            var order = 2;

            var rows123 = Pict.GenerateIndices(counts, (uint)order, seed: 123);
            var rows456 = Pict.GenerateIndices(counts, (uint)order, seed: 456);

            AssertAllPairsPresent(rows123, counts, order);
            AssertAllPairsPresent(rows456, counts, order);

            Assert.AreEqual(rows123.Count, rows456.Count, 2, "Mismatched row count."); // Count should be relatively close. A different seed may cause a difference in row count.

            foreach (var (row123, row456) in rows123.Zip(rows456))
            {
                Assert.HasCount(row123.Count, row456, "Mismatched count within row.");
            }

            var flattened123 = rows123.SelectMany(l => l);
            var flattened456 = rows456.SelectMany(l => l);


            Assert.IsTrue(flattened123.Zip(flattened456).Any(pair => pair.First != pair.Second)); // there should be at least one difference between seeds
        }


        [TestMethod]
        [DynamicData(nameof(AllPairsCases))]
        public void TestAllPairsPresent(List<int> valueCounts, int order, int seed)
        {
            var rows = Pict.GenerateIndices(valueCounts, (uint)order);
            AssertAllPairsPresent(rows, valueCounts, order);
        }

        [TestMethod]
        public void TestCombinations()
        {
            List<IList<string>> parameterOptions =
            [
                ["A1"],
                ["B1", "B2", "B3"],
                ["C1", "C2", "C3"],
                ["D1", "D2", "D3", "D4"]
            ];

            var order = 2;

            var combinations = Pict.GenerateCombinations(parameterOptions, order);

            List<List<string>> expected =
            [
                ["A1", "B1", "C2", "D2"],
                ["A1", "B2", "C1", "D3"],
                ["A1", "B3", "C1", "D1"],
                ["A1", "B3", "C3", "D2"],
                ["A1", "B2", "C2", "D1"],
                ["A1", "B2", "C1", "D2"],
                ["A1", "B3", "C2", "D3"],
                ["A1", "B1", "C2", "D4"],
                ["A1", "B2", "C1", "D4"],
                ["A1", "B1", "C3", "D1"],
                ["A1", "B2", "C3", "D3"],
                ["A1", "B3", "C3", "D4"],
                ["A1", "B1", "C1", "D3"]
            ];

            Assert.HasCount(expected.Count, combinations, "Unexpected combination count.");

            foreach (var (combination, expectedCombination) in combinations.Zip(expected))
            {
                Assert.HasCount(parameterOptions.Count, combination, "Unexpected parameter count in a combination.");
                CollectionAssert.AreEqual(expectedCombination, combination);
            }
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
