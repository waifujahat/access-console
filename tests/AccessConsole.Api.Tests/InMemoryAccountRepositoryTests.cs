/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Persistence / Tests
 *
 * Validates in-memory account persistence
 */

using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;
using Xunit;

namespace Waifujahat
{
    namespace AccessConsole.Api.Tests
    {
        public sealed class InMemoryAccountRepositoryTests
        {
            [Fact]
            public async Task AddAsync_withValidAccount_StoresAccount()
            {
                var Repositori = new InMemoryAccountRepository();

                var AkunBaru = Account.Create(
                    "repository@example.com",
                    "Repository Test");

                await Repositori.AddAsync(AkunBaru);

                var AkunDitemukan = await Repositori.GetByIdAsync(
                    AkunBaru.Id);

                Assert.Same(AkunBaru, AkunDitemukan);
            }

            [Fact]
            public async Task GetByIdAsync_withUnknownId_ReturnsNull()
            {
                var Repositori = new InMemoryAccountRepository();

                var AkunDitemukan = await Repositori.GetByIdAsync(
                    Guid.NewGuid());

                Assert.Null(AkunDitemukan);
            }

            [Fact]
            public async Task AddAsync_withDuplicateId_ThrowsException()
            {
                var Repositori = new InMemoryAccountRepository();

                var AkunBaru = Account.Create(
                    "repository@example.com",
                    "Repository Test");

                await Repositori.AddAsync(AkunBaru);

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Repositori.AddAsync(AkunBaru));
            }

            [Fact]
            public async Task AddAsync_withCanceledToken_ThrowsOperationCanceledException()
            {
                var Repositori = new InMemoryAccountRepository();

                var AkunBaru = Account.Create(
                    "repository@example.com",
                    "Repository Test");

                using var SumberPembatalan = new CancellationTokenSource();

                SumberPembatalan.Cancel();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    async () => await Repositori.AddAsync(
                        AkunBaru,
                        SumberPembatalan.Token));

                var AkunDitemukan = await Repositori.GetByIdAsync(AkunBaru.Id);

                Assert.Null(AkunDitemukan);
            }

            [Fact]
            public async Task GetByIdAsync_withCanceledToken_ThrowsOperationCanceledException()
            {
                var Repositori = new InMemoryAccountRepository();

                using var SumberPembatalan = new CancellationTokenSource();

                SumberPembatalan.Cancel();

                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    async () => await Repositori.GetByIdAsync(
                        Guid.NewGuid(),
                        SumberPembatalan.Token));
            }
        }
    }
}
