using System;
using System.Linq;
using BudgetBuddy.Domain;
using BudgetBuddy.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BudgetBuddy.App.Tests
{
    public class TransactionsRepositoryTests
    {
        [Fact]
        public void TryAdd_ReturnsTrue_ForNewId_AndFalse_ForDuplicate()
        {
            var repo = new TransactionsRepository();
            var tx = new Transaction { Id="1", Timestamp=new DateTime(2024,11,10), Payee="A", Amount=1m, Currency="USD", Category="X" };

            repo.TryAdd(tx).Should().BeTrue();
            repo.TryAdd(tx).Should().BeFalse();
        }

        [Fact]
        public void Remove_Deletes_And_ContainsReflectsState()
        {
            var repo = new TransactionsRepository();
            var tx = new Transaction { Id="2", Timestamp=new DateTime(2024,11,10), Payee="B", Amount=1m, Currency="USD", Category="Y" };
            repo.TryAdd(tx).Should().BeTrue();
            repo.Contains("2").Should().BeTrue();

            repo.Remove("2").Should().BeTrue();
            repo.Contains("2").Should().BeFalse();
        }

        [Fact]
        public void GetAll_Returns_Ordered_By_NumericId()
        {   
            var repo = new TransactionsRepository();
            repo.TryAdd(new Transaction { Id="10", Timestamp=DateTime.UtcNow, Payee="A", Amount=0, Currency="USD", Category="C" });
            repo.TryAdd(new Transaction { Id="2", Timestamp=DateTime.UtcNow, Payee="B", Amount=0, Currency="USD", Category="C" });
            repo.TryAdd(new Transaction { Id="1", Timestamp=DateTime.UtcNow, Payee="C", Amount=0, Currency="USD", Category="C" });

            var ordered = repo.GetAll().Select(t => t.Id).ToArray();
            ordered.Should().Equal("1","2","10");
        }
    }
}
