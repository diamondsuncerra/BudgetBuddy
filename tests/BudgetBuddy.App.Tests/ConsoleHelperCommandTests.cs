using System;
using BudgetBuddy.Domain;
using BudgetBuddy.App;
using Moq;
using Xunit;

namespace BudgetBuddy.App.Tests
{
    public class ConsoleHelperCommandTests
    {
        [Fact]
        public void SetCategory_Updates_Existing_Transaction()
        {
            var repo = new Mock<IRepository<Transaction, string>>();
            var tx = new Transaction { Id="2", Timestamp=new DateTime(2024,11,12), Payee="Lidl", Amount=-50m, Currency="USD", Category="Groceries" };

            repo.Setup(r => r.Contains("2")).Returns(true);
            repo.Setup(r => r.TryGet("2", out tx)).Returns(true);

            ConsoleHelper.SetCategory(["2", "Dining"], repo.Object);

            Assert.Equal("Dining", tx.Category);
            repo.Verify(r => r.TryGet("2", out tx), Times.Once);
        }

        [Fact]
        public void SetCategory_When_Id_Not_Found_Does_Not_TryGet()
        {
            var repo = new Mock<IRepository<Transaction, string>>();
            repo.Setup(r => r.Contains("999")).Returns(false);

            ConsoleHelper.SetCategory(["999", "Anything"], repo.Object);

            Transaction? ignored;
            repo.Verify(r => r.TryGet("999", out ignored!), Times.Never);
        }
    }
}
