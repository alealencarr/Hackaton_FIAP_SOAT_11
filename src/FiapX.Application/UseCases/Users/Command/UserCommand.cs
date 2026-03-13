namespace FiapX.Application.UseCases.Users.Command;

public class RegisterUserCommand
{
    public RegisterUserCommand(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = password;
    }

    public RegisterUserCommand() { }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginUserCommand
{
    public LoginUserCommand(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public LoginUserCommand() { }

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
