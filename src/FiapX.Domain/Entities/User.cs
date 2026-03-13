using System.Security.Cryptography;
using System.Text;

namespace FiapX.Domain.Entities
{
    public class User
    {
        public User(Guid id, string name, string email, string passwordHash, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = createdAt;
        }

        public User(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("O nome é obrigatório para criar um usuário.");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException("O e-mail é obrigatório para criar um usuário.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("A senha é obrigatória para criar um usuário.");

            if (password.Length < 6)
                throw new ArgumentException("A senha deve ter no mínimo 6 caracteres.");

            if (!IsValidEmail(email))
                throw new ArgumentException("E-mail inválido.");

            Id = Guid.NewGuid();
            Name = name;
            Email = email.ToLowerInvariant();
            PasswordHash = HashPassword(password);
            CreatedAt = DateTime.UtcNow;
        }

        public User() { }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public bool ValidatePassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
