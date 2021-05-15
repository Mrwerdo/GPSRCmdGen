using Xunit;

namespace RoboCup.AtHome.CommandGenerator.Tests
{
    public class ProductionRuleTest
    {
        [Theory]
        [InlineData("a b c")]
        public void LiteralTest(string sentence)
        {
            var result = ProductionRule.ExpandBranchExpression(sentence);
            AssertEquivalent(new string[] { "a b c" }, result);
        }

        [Theory]
        [InlineData("a | b")]
        public void LiteralWithBar(string sentence)
        {
            var result = ProductionRule.ExpandBranchExpression(sentence);
            AssertEquivalent(new string[] { "a", "b" }, result);
        }

        [Theory]
        [InlineData("(a | b) c (d | e)")]
        public void AdjacentBranches(string sentence)
        {
            var result = ProductionRule.ExpandBranchExpression(sentence);
            AssertEquivalent(new string[] { "a c d", "a c e", "b c d", "b c e" }, result);
        }

        [Theory]
        [InlineData("a (b | c) d")]
        [InlineData("a (b|c) d")]
        [InlineData("a (b| c ) d")]
        [InlineData("a ( b |c) d")]
        public void StartsAndEndsWithLiteral(string sentence)
        {
            var result = ProductionRule.ExpandBranchExpression(sentence);
            AssertEquivalent(new string[] { "a b d", "a c d" }, result);
        }

        [Theory]
        [InlineData("((a | b) c | d (e | f))")]
        public void NestedParenthesis(string sentence)
        {
            var result = ProductionRule.ExpandBranchExpression(sentence);
            AssertEquivalent(new string[] { "a c", "b c", "d e", "d f" }, result);
        }

        [Theory]
        [InlineData("a | (b | c)")]
        public void SimpleNestedParenthesis(string sentence)
        {
            var result = ProductionRule.ExpandBranchExpression(sentence);
            AssertEquivalent(new string[] { "a b", "a c" }, result);
        }

        [Theory]
        [InlineData("(")]
        [InlineData(")")]
        [InlineData(")(")]
        [InlineData("())")]
        [InlineData("(()")]
        public void MismatcheParenthesis(string sentence) 
        {
            string[] result = ProductionRule.ExpandBranchExpression(sentence);
            Assert.Null(result);
        }

        // Is there a xUnit method for this?
        private static void AssertEquivalent(string[] expected, string[] actual)
        {
            foreach (string s in expected)
            {
                Assert.Contains(s, actual);
            }
            Assert.Equal(expected.Length, actual.Length);
        }
    }
}