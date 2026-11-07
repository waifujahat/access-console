/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Persistence
 *
 * Configures account persistence using Entity Framework Core
 */

using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Domain.Accounts;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Persistence
    {
        public sealed class AccessConsoleDbContext : DbContext
        {
            public AccessConsoleDbContext(
                DbContextOptions<AccessConsoleDbContext> Options)
                : base(Options)
            {
            }

            public DbSet<Account> Accounts => Set<Account>();

            protected override void OnModelCreating(ModelBuilder ModelBuilder)
            {
                base.OnModelCreating(ModelBuilder);

                ModelBuilder.HasPostgresExtension("citext"); // Case-insensitive text

                ModelBuilder.Entity<Account>(Entitas =>
                {
                    Entitas.ToTable("accounts");

                    Entitas.HasKey(Akun => Akun.Id);

                    Entitas.Property(Akun => Akun.Id)
                        .HasColumnName("id")
                        .ValueGeneratedNever();

                    Entitas.Property(Akun => Akun.Email)
                        .HasColumnName("email")
                        .HasColumnType("citext")
                        .HasMaxLength(254)
                        .IsRequired();

                    Entitas.Property(Akun => Akun.DisplayName)
                        .HasColumnName("display_name")
                        .HasMaxLength(100)
                        .IsRequired();

                    Entitas.Property(Akun => Akun.CreatedatUTC)
                        .HasColumnName("created_at_utc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired();

                    Entitas.HasIndex(Akun => Akun.Email)
                        .IsUnique();
                });
            }
        }
    }
}
