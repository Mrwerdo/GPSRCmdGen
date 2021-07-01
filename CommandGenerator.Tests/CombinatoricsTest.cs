using Xunit;

namespace RoboCup.AtHome.CommandGenerator.Tests
{
    public class CombinatoricsTest
    {
        [Fact] 
        public void TestFinalOverflowIncrement() {
            var lengths = new int[5] { 10, 10, 10, 10, 10 };
            var indices = new int[5] { 9, 9, 9, 9, 9 };
            var index = 4;
            var result = Combinatorics.IncrementIndices(lengths, indices, ref index);
            Assert.True(result);
            Assert.Equal(0, index);
        }

        [Fact]
        public void TestOverflowIncrement() {
            var lengths = new int[5] { 10, 10, 10, 10, 10 };
            var indices = new int[5] { 9, 9, 9, 0, 9 };
            var index = 4;
            var result = Combinatorics.IncrementIndices(lengths, indices, ref index);
            Assert.False(result);
            Assert.Equal(3, index);
            Assert.Equal(1, indices[index]);
        }
    }
}