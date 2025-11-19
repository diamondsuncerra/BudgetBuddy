using System;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BudgetBuddy.App.Tests
{
    public class TransactionFactoryTests
    {
        [Fact]
        public void TryCreate_ValidRow_MapsAllFields()
        {
            var line = "1,2024-11-12,Lidl,-50,USD,Groceries";
            var result = TransactionFactory.TryCreate(line);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Id.Should().Be("1");
            result.Value!.Timestamp.Should().Be(new DateTime(2024,11,12));
            result.Value!.Payee.Should().Be("Lidl");
            result.Value!.Amount.Should().Be(-50m);
            result.Value!.Currency.Should().Be("USD");
            result.Value!.Category.Should().Be("Groceries");
        }

        [Theory]
        [InlineData("1,12-11-2024,Lidl,-50,USD,Groceries", "Invalid Timestamp")]
        [InlineData("1,2024-11-12,Lidl,50,50,USD,Groceries", "Wrong column count")]
        [InlineData("1,2024-11-12,Lidl,505feer0,USD,Groceries", "Invalid Amount")]
        [InlineData(",2024-11-12,Lidl,-50,USD,Groceries", "Missing Id")]
        public void TryCreate_BadDateAmountOrId_FailsWithMessage(string line, string expectedErrorFragment)
        {
            var result = TransactionFactory.TryCreate(line);
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain(expectedErrorFragment);
        }



        [Fact]
        public void TryCreate_AmountOutOfRange_Fails()
        {
            var line = "1,2024-11-12,Lidl,1000001,USD,Groceries";
            var result = TransactionFactory.TryCreate(line);
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Amount out of range");
        }
    }
}
