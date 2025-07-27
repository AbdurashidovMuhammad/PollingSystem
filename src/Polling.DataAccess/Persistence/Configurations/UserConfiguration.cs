using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Polling.Core.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Polling.DataAccess.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(u => u.PasswordSalt)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasData(GetSeedUser());
    }
    private static string Encrypt(string password, string salt)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password: password,
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: 1000,
            hashAlgorithm: HashAlgorithmName.SHA256);

        return Convert.ToBase64String(algorithm.GetBytes(32));
    }

    private static User GetSeedUser()
    {
        const string seedSalt = "superadmin-seed-salt";
        const string seedPassword = "superadmin123";
        var superAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        return new User
        {
            Id = superAdminId,
            FullName = "Super Admin",
            Email = "superadmin@polling.com",
            PasswordSalt = seedSalt,
            PasswordHash = Encrypt(seedPassword, seedSalt),
            IsEmailVerified = true,

            Role = Polling.Core.Enums.UserRole.SuperAdmin // Agar enum bo'lsa
        };
    }
}
