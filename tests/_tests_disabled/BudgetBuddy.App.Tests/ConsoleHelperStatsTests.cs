using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBuddy.Domain;
using BudgetBuddy.App;
using Moq;
using FluentAssertions;
using Xunit;

namespace BudgetBuddy.App.Tests
{
    public class ConsoleHelperStatsTests
    {
        private static IEnumerable<Transaction> Sample() => new[]
        {
            new Transaction { Id="1", Timestamp=new DateTime(2024,11,10), Payee="Company",  Amount=+1500m, Currency="USD", Category="Salary" },
            new Transaction { Id="2", Timestamp=new DateTime(2024,11,12), Payee="Lidl",    Amount=-50m,  Currency="USD", Category="Groceries" },
            new Transaction { Id="3", Timestamp=new DateTime(2024,11,13), Payee="Metro",   Amount=-20m,  Currency="USD", Category="Transport" },
            new Transaction { Id="4", Timestamp=new DateTime(2024,10,05), Payee="Carrefour",Amount=-100m, Currency="USD", Category="Groceries" },
        };

        [Fact]
        public void GetIncomeExpenseNetForMonth_Computes_Correctly()
        {
            var repo = new Mock<IRepository<Transaction, string>>();
            repo.Setup(r => r.GetAll()).Returns(Sample());

            var (income, expense, net) = ConsoleHelper.GetIncomeExpenseNetForMonth("2024-11", repo.Object);

            income.Should().Be(1500m);
            expense.Should().Be(70m);
            net.Should().Be(1430m);
        }

        [Fact]
        public void GetTopExpenseCategoriesForMonth_Orders_Descending_By_Absolute()
        {
            var repo = new Mock<IRepository<Transaction, string>>();
            repo.Setup(r => r.GetAll()).Returns(Sample());

            var top = ConsoleHelper.GetTopExpenseCategoriesForMonth("2024-11", repo.Object, 3).ToList();

            top.Should().HaveCount(2);
            top[0].Category.Should().Be("Groceries");
            top[0].Total.Should().Be(-50m);
            top[1].Category.Should().Be("Transport");
            top[1].Total.Should().Be(-20m);
        }
    }
}
