using System;
using System.IO;
using BudgetBuddy.Domain;
using FluentAssertions;
using Xunit;

namespace BudgetBuddy.App.Tests
{
    public class LoggerAndResultTests
    {
        [Fact]
        public void Logger_Writes_With_Prefixes()
        {
            var sw = new StringWriter();
            var original = Console.Out;
            Console.SetOut(sw);
            try
            {
                Logger.Info("info");
                Logger.Warn("warn");
                Logger.Error("error");
                Logger.GreenInfo("ok");

                var output = sw.ToString();
                output.Should().Contain("[INFO]: info");
                output.Should().Contain("[WARNING]: warn");
                output.Should().Contain("[ERROR]: error");
                output.Should().Contain("[SUCCESS]: ok");
            }
            finally
            {
                Console.SetOut(original);
            }
        }

        [Fact]
        public void Result_Ok_And_Fail_Behave_As_Expected()
        {
            var ok = Result<int>.Ok(42);
            ok.IsSuccess.Should().BeTrue();
            ok.Error.Should().BeEmpty();
            ok.Value.Should().Be(42);

            var fail = Result<string>.Fail("bad");
            fail.IsSuccess.Should().BeFalse();
            fail.Error.Should().Be("bad");
            fail.Value.Should().BeNull();
        }
    }
}
