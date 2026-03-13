using FiapX.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace FiapX.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Deve_Criar_Usuario_Valido()
    {
        var nome = "João Silva";
        var email = "joao@email.com";
        var senha = "senha123";

        var user = new User(nome, email, senha);

        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be(nome);
        user.Email.Should().Be(email.ToLowerInvariant());
        user.PasswordHash.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Nome_Invalido(string nomeInvalido)
    {
        Action act = () => new User(nomeInvalido, "email@email.com", "senha123");

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*nome*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Email_Vazio(string emailInvalido)
    {
        Action act = () => new User("João", emailInvalido, "senha123");

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*e-mail*");
    }

    [Theory]
    [InlineData("email_invalido")]
    [InlineData("email@")]
    [InlineData("@email.com")]
    public void Deve_Lancar_Excecao_Quando_Email_Formato_Invalido(string emailInvalido)
    {
        Action act = () => new User("João", emailInvalido, "senha123");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*E-mail inválido*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Senha_Vazia(string senhaInvalida)
    {
        Action act = () => new User("João", "email@email.com", senhaInvalida);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*senha*");
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Muito_Curta()
    {
        Action act = () => new User("João", "email@email.com", "12345");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*mínimo 6 caracteres*");
    }

    [Fact]
    public void Deve_Validar_Senha_Correta()
    {
        var senha = "senha123";
        var user = new User("João", "email@email.com", senha);

        var resultado = user.ValidatePassword(senha);

        resultado.Should().BeTrue();
    }

    [Fact]
    public void Deve_Invalidar_Senha_Incorreta()
    {
        var user = new User("João", "email@email.com", "senha123");

        var resultado = user.ValidatePassword("senhaerrada");

        resultado.Should().BeFalse();
    }

    [Fact]
    public void Deve_Converter_Email_Para_Minusculo()
    {
        var user = new User("João", "JOAO@EMAIL.COM", "senha123");

        user.Email.Should().Be("joao@email.com");
    }

    [Fact]
    public void Deve_Reconstituir_Usuario_Completo()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var passwordHash = "hash123";

        var user = new User(id, "João", "joao@email.com", passwordHash, createdAt);

        user.Id.Should().Be(id);
        user.Name.Should().Be("João");
        user.Email.Should().Be("joao@email.com");
        user.PasswordHash.Should().Be(passwordHash);
        user.CreatedAt.Should().Be(createdAt);
    }
}
