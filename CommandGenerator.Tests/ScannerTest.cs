using Xunit;

namespace RoboCup.AtHome.CommandGenerator.Tests
{
	public class ScannerTests
	{
		[Fact]
		public void WildcardThenNonTerminalThenWildcard()
		{
			var input = "{beacon}, $vbfind {wildcard}";
			var result = Scanner.SplitRule(input);
			Assert.Equal(new string[] {
				"{beacon}",
				", ",
				"$vbfind",
				" ",
				"{wildcard}"
			}, result);
		}

		[Theory]
		[InlineData("$Main")]
		[InlineData("Main")]
		[InlineData("{object where Category=\"tableware\"}")]
		public void DoesNothing(string input)
		{
			var result = Scanner.SplitRule(input);
			Assert.Equal(new string[] { input }, result);
		}

		[Fact]
		public void SplitTokensSeparatedByLiteralText()
		{
			var input = "$singleI, $single, and $single";
			var result = Scanner.SplitRule(input);
			Assert.Equal(new string[] {
				"$singleI",
				", ",
				"$single",
				", and ",
				"$single"
			}, result);
		}

		[Fact]
		public void RespectsNestedParenthesis()
		{
			var input = "$vbfind {name meta: {pron sub} is standing at the {beacon}}";
			var result = Scanner.SplitRule(input);
			Assert.Equal(new string[] {
				"$vbfind",
				" ",
				"{name meta: {pron sub} is standing at the {beacon}}"
			}, result);
		}

		[Fact]
		public void ContainsProductionInsideWildcard()
		{
			var input = "say something {void meta: When asked, reply to the robot: \"$whattosay\" }";
			var result = Scanner.SplitRule(input);
			Assert.Equal(new string[] {
				"say something ",
				"{void meta: When asked, reply to the robot: \"$whattosay\" }",
			}, result);
		}
	}
}