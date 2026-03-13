using FiapX.Application.Controllers.Users;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Domain.Entities;
using FiapX.Shared.DTO.User.Input;
using FiapX.Shared.DTO.User.Request;
using FluentAssertions;
using Moq;
using Xunit;

namespace FiapX.UnitTests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserDataSource> _dataSourceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _dataSourceMock = new Mock<IUserDataSource>();
        _jwtServiceMock = new Mock<IJwtService>();
        _controller = new UserController(_dataSourceMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Register_Deve_Retornar_Sucesso_Quando_Valido()
    {
        var request = new UserRegisterRequestDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Password = "senha123"
        };

        _dataSourceMock.Setup(x => x.ExistsByEmail(request.Email)).ReturnsAsync(false);

        var result = await _controller.Register(request);

        result.Succeeded.Should().BeTrue();
        result.Messages.Should().Contain("Usuário cadastrado com sucesso!");
        result.Data.Name.Should().Be(request.Name);
        result.Data.Email.Should().Be(request.Email.ToLowerInvariant());

        _dataSourceMock.Verify(x => x.Create(It.IsAny<UserInputDto>()), Times.Once);
    }

    [Fact]
    public async Task Register_Deve_Retornar_Erro_Quando_Email_Ja_Existe()
    {
        var request = new UserRegisterRequestDto
        {
            Name = "João Silva",
            Email = "joao@email.com",
            Password = "senha123"
        };

        _dataSourceMock.Setup(x => x.ExistsByEmail(request.Email)).ReturnsAsync(true);

        var result = await _controller.Register(request);

        result.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain(m => m.Contains("Já existe um usuário cadastrado com este e-mail."));
    }

    [Fact]
    public async Task Login_Deve_Retornar_Sucesso_Quando_Credenciais_Validas()
    {
        var request = new UserLoginRequestDto
        {
            Email = "joao@email.com",
            Password = "senha123"
        };

        var user = new User("João Silva", request.Email, request.Password);
        var userDto = new UserInputDto(user.Id, user.Name, user.Email, user.PasswordHash, user.CreatedAt);

        _dataSourceMock.Setup(x => x.GetByEmail(request.Email)).ReturnsAsync(userDto);
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(("token_jwt_valido", DateTime.UtcNow.AddHours(24)));

        var result = await _controller.Login(request);

        result.Succeeded.Should().BeTrue();
        result.Messages.Should().Contain("Login realizado com sucesso!");
        result.Data.Token.Should().Be("token_jwt_valido");
        result.Data.User.Email.Should().Be(request.Email.ToLowerInvariant());
    }

    [Fact]
    public async Task Login_Deve_Retornar_Erro_Quando_Email_Nao_Encontrado()
    {
        var request = new UserLoginRequestDto
        {
            Email = "naoexiste@email.com",
            Password = "senha123"
        };

        _dataSourceMock.Setup(x => x.GetByEmail(request.Email)).ReturnsAsync((UserInputDto?)null);

        var result = await _controller.Login(request);

        result.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("E-mail ou senha inválidos.");
    }

    [Fact]
    public async Task Login_Deve_Retornar_Erro_Quando_Senha_Incorreta()
    {
        var request = new UserLoginRequestDto
        {
            Email = "joao@email.com",
            Password = "senha_errada"
        };

        var user = new User("João Silva", request.Email, "senha_correta");
        var userDto = new UserInputDto(user.Id, user.Name, user.Email, user.PasswordHash, user.CreatedAt);

        _dataSourceMock.Setup(x => x.GetByEmail(request.Email)).ReturnsAsync(userDto);

        var result = await _controller.Login(request);

        result.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("E-mail ou senha inválidos.");
    }
}
