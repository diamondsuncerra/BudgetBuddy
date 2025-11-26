using System;
using System.Linq;
using BudgetBuddy.Domain;
using FluentAssertions;
using Xunit;

namespace BudgetBuddy.App.Tests
{
    public class ExtensionsTests
    {
        [Fact]
        public void ToMoney_Formats_Invariantly()
        {
            1234.5m.ToMoney("USD").Should().Be("1,234.50 USD");
        }

        [Fact]
        public void MonthKey_Returns_yyyy_MM()
        {
            new DateTime(2024,11,13).MonthKey().Should().Be("2024-11");
        }

        [Fact]
        public void TryDate_And_TryDec_BasicValidation()
        {
            Extensions.TryDate("2024-11-12").IsSuccess.Should().BeTrue();
            Extensions.TryDate("12/11/2024").IsSuccess.Should().BeFalse();
            Extensions.TryDec("42.5").IsSuccess.Should().BeTrue();
            Extensions.TryDec("oops").IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void ToPrettyTable_Returns_Formatted_String()
        {
            var input = new[] { ("Groceries", -50m), ("Transport", -20m) };
            var table = input.ToPrettyTable("USD");
            table.Should().Contain("Top Expense Categories:")
                 .And.Contain("Groceries")
                 .And.Contain("Transport");
        }
    }
}
