/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Persistence
 *
 * Configures account persistence using Entity Framework Core
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Authentication;
using Waifujahat.AccessConsole.Api.Infrastructure.Identity;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Persistence
    {
        public sealed class AccessConsoleDbContext : IdentityDbContext<AccessConsoleIdentityUser, IdentityRole<Guid>, Guid>
        {
            public AccessConsoleDbContext(
                DbContextOptions<AccessConsoleDbContext> Options)
                : base(Options)
            {
            }

            public DbSet<Account> Accounts => Set<Account>();

            public DbSet<RevokedAccessToken> RevokedAccessTokens =>
            Set<RevokedAccessToken>();

            protected override void OnModelCreating(ModelBuilder ModelBuilder)
            {
                base.OnModelCreating(ModelBuilder);

                ModelBuilder.HasPostgresExtension("citext"); // Case-insensitive text

                ModelBuilder.Entity<RevokedAccessToken>(Entitas =>
                {
                    Entitas.ToTable("revoked_access_tokens");

                    Entitas.HasKey(Token => Token.TokenHash);

                    Entitas.Property(Token => Token.TokenHash)
                        .HasColumnName("token_hash")
                        .HasMaxLength(64)
                        .ValueGeneratedNever()
                        .IsRequired();

                    Entitas.Property(Token => Token.RevokedAtUtc)
                        .HasColumnName("revoked_at_utc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired();

                    Entitas.Property(Token => Token.ExpiresAtUtc)
                        .HasColumnName("expires_at_utc")
                        .HasColumnType("timestamp with time zone")
                        .IsRequired();

                    Entitas.HasIndex(Token => Token.ExpiresAtUtc);
                });

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

                    ModelBuilder.Entity<AccessConsoleIdentityUser>(Entitas =>
                    {
                        Entitas.HasOne<Account>()
                            .WithOne()
                            .HasForeignKey<AccessConsoleIdentityUser>(
                                Pengguna => Pengguna.Id)
                            .OnDelete(DeleteBehavior.Cascade);
                    });
                });
            }
        }
    }
}
