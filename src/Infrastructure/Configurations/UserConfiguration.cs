using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Configurations.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Configurations;

/// <summary>
/// Configura la persistencia del perfil de usuario.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.CreatedAt).HasColumnName("created_at");
        builder.Property(user => user.UpdatedAt).HasColumnName("updated_at");
        builder.Property(user => user.Email)
            .HasConversion(new ValueConverter<Email, string>(
                email => email.Value,
                value => new Email(value)))
            .HasColumnName("email")
            .IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.OwnsOne(user => user.Name, name =>
        {
            name.Property(value => value.FirstName).HasColumnName("first_name").IsRequired();
            name.Property(value => value.LastName).HasColumnName("last_name").IsRequired();
        });
        builder.Property(user => user.PhoneNumber)
            .HasConversion(new ValueConverter<PhoneNumber?, string?>(
                phone => phone == null ? null : phone.Value,
                value => value == null ? null : new PhoneNumber(value)))
            .HasColumnName("phone_number");
        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasColumnName("role")
            .IsRequired();
        builder.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
    }
}
