using FluentAssertions;
using FiapX.Shared.Result;
using Xunit;

namespace UnitTests.Shared
{
    public class CommandResultTests
    {

        [Fact]
        public void Should_Create_Success_Result_Base()
        {
            var result = CommandResult.Success();
            result.Succeeded.Should().BeTrue();
            result.Messages.Should().BeEmpty();

            var resultMsg = CommandResult.Success("Sucesso total");
            resultMsg.Succeeded.Should().BeTrue();
            resultMsg.Messages.Should().Contain("Sucesso total");
        }

        [Fact]
        public async Task Should_Create_Success_Result_Base_Async()
        {
            var result = await CommandResult.SuccessAsync();
            result.Succeeded.Should().BeTrue();

            var resultMsg = await CommandResult.SuccessAsync("Sucesso Async");
            resultMsg.Succeeded.Should().BeTrue();
            resultMsg.Messages.Should().Contain("Sucesso Async");
        }

        [Fact]
        public void Should_Create_Fail_Result_Base()
        {
            var result = CommandResult.Fail();
            result.Succeeded.Should().BeFalse();

            var resultMsg = CommandResult.Fail("Erro simples");
            resultMsg.Succeeded.Should().BeFalse();
            resultMsg.Messages.Should().Contain("Erro simples");

            var msgs = new List<string> { "Erro 1", "Erro 2" };
            var resultList = CommandResult.Fail(msgs);
            resultList.Succeeded.Should().BeFalse();
            resultList.Messages.Should().HaveCount(2);
        }

        [Fact]
        public async Task Should_Create_Fail_Result_Base_Async()
        {
            var result = await CommandResult.FailAsync();
            result.Succeeded.Should().BeFalse();

            var resultMsg = await CommandResult.FailAsync("Erro Async");
            resultMsg.Succeeded.Should().BeFalse();

            var msgs = new List<string> { "E1", "E2" };
            var resultList = await CommandResult.FailAsync(msgs);
            resultList.Succeeded.Should().BeFalse();
        }

        [Fact]
        public void Should_Create_Conflict_Result_Base()
        {
            var result = CommandResult.ConflictReturn("Conflito detectado");
            result.Conflict.Should().BeTrue();
            result.Messages.Should().Contain("Conflito detectado");
        }


        [Fact]
        public void Should_Create_Success_Result_Generic()
        {
            var r1 = CommandResult<int>.Success();
            r1.Succeeded.Should().BeTrue();

            var r2 = CommandResult<int>.Success("Msg");
            r2.Succeeded.Should().BeTrue();

            var r3 = CommandResult<int>.Success(10);
            r3.Succeeded.Should().BeTrue();
            r3.Data.Should().Be(10);

            var r4 = CommandResult<int>.Success(20, "Ok");
            r4.Data.Should().Be(20);
            r4.Messages.Should().Contain("Ok");

            var r5 = CommandResult<int>.Success(30, new List<string> { "Ok1" });
            r5.Data.Should().Be(30);
            r5.Messages.Should().HaveCount(1);
        }

        [Fact]
        public async Task Should_Create_Success_Result_Generic_Async()
        {
            var r1 = await CommandResult<string>.SuccessAsync("Msg");
            r1.Succeeded.Should().BeTrue();

            var r2 = await CommandResult<int>.SuccessAsync(99);
            r2.Data.Should().Be(99);

            var r3 = await CommandResult<int>.SuccessAsync(100, "Top");
            r3.Data.Should().Be(100);
        }

        [Fact]
        public void Should_Create_Fail_Result_Generic()
        {
            var r1 = CommandResult<int>.Fail();
            r1.Succeeded.Should().BeFalse();

            var r2 = CommandResult<int>.Fail("Deu ruim");
            r2.Messages.Should().Contain("Deu ruim");

            var r3 = CommandResult<int>.Fail(0);
            r3.Succeeded.Should().BeFalse();

            var r4 = CommandResult<int>.Fail(0, "Erro com dados");
            r4.Messages.Should().Contain("Erro com dados");

            var r5 = CommandResult<int>.Fail(new List<string> { "E" });
            r5.Messages.Should().HaveCount(1);

            var r6 = CommandResult<int>.Fail(0, new List<string> { "E" });
            r6.Messages.Should().HaveCount(1);
        }

        [Fact]
        public async Task Should_Create_Fail_Result_Generic_Async()
        {
            var r1 = await CommandResult<int>.FailAsync();
            r1.Succeeded.Should().BeFalse();

            var r2 = await CommandResult<int>.FailAsync("Erro");
            r2.Succeeded.Should().BeFalse();

            var r3 = await CommandResult<int>.FailAsync(new List<string> { "E" });
            r3.Messages.Should().NotBeEmpty();

            var r4 = await CommandResult<int>.FailAsync(1, new List<string> { "E" });
            r4.Data.Should().Be(1);
            r4.Succeeded.Should().BeFalse();

            var r5 = await CommandResult<int>.FailAsync(1, "E");
            r5.Data.Should().Be(1);

            var r6 = await CommandResult<int>.FailAsync(500);
            r6.Data.Should().Be(500);
            r6.Succeeded.Should().BeFalse();
        }

        [Fact]
        public void Should_Create_Conflict_Result_Generic()
        {
            var result = CommandResult<int>.ConflictReturn("Conflito Genérico");
            result.Conflict.Should().BeTrue();
            result.Messages.Should().Contain("Conflito Genérico");
        }
    }
}