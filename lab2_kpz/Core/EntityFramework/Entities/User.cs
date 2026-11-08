using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace lab2_kpz.Entities;

public sealed class User
{
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateOnly? UserBirthDate { get; set; }
    public string? UserPhoneNumber { get; set; }

    public ICollection<DiaryEntry> DiaryEntries { get; set; } = new List<DiaryEntry>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UserEmail)
            .IsRequired()
            .HasMaxLength(120)
            .IsUnicode();

        builder.HasIndex(u => u.UserEmail)
            .IsUnique();

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(u => u.UserBirthDate)
            .HasColumnType("date");

        builder.Property(u => u.UserPhoneNumber)
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.HasMany(u => u.DiaryEntries)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Reports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
