using System.Text.RegularExpressions;
using RoboCup.AtHome.GPSRCmdGen;
using Xunit;

namespace RoboCup.AtHome.GPSRCmdGen.Tests
{
    public class ReadLineTests
    {
        [Theory]
        [InlineData("s   123")]
        [InlineData("s 123")]
        [InlineData("se 123")]
        [InlineData("see 123")]
        [InlineData("seed 123")]
        public void SeedTest(string input) 
        {
            var result = Keyboard.ExtractSeed(input);
            Assert.Equal(123, result);
        }

        [Theory]
        [InlineData("s")]
        [InlineData("s ")]
        [InlineData("se ")]
        [InlineData("se not a number")]
        public void SeedFailureTest(string input) 
        {
            var result = Keyboard.ExtractSeed(input);
            Assert.Null(result);
        }

        [Fact]
        public void ImplicitGenerateTripleTest()
        {
            var result = Keyboard.ExtractDollarCommand("$main:1 $task:2 carry:3");
            Assert.Contains(("main", 1), result);
            Assert.Contains(("task", 2), result);
            Assert.Contains(("carry", 3), result);
        }

        [Fact]
        public void ImplicitGenerateMainTest()
        {
            var result = Keyboard.ExtractDollarCommand("$main");
            Assert.Contains(("main", null), result);
        }

        [Theory]
        [InlineData("$")]
        [InlineData("$:")]
        [InlineData("$:1")]
        [InlineData("$:1 $:2")]
        [InlineData("$main:1 $:2")]
        [InlineData("$:1 $test:2")]
        [InlineData("main:1 task:2")]
        [InlineData(" main")]
        [InlineData("main")]
        public void ImplicitGenerateRejected(string line)
        {
            var result = Keyboard.ExtractDollarCommand(line);
            Assert.Empty(result);
        }
    }
}