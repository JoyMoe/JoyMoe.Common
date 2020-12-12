using System;
using System.Globalization;
using System.Threading.Tasks;
using JoyMoe.Common.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

#nullable disable
namespace JoyMoe.Common.Session.Repository
{
    /// <summary>
    /// This provides an storage mechanic to preserve identity information in the Repository while only sending a simple identifier key to the client.
    /// </summary>
    public class RepositoryTicketStore<TUser, TSession, TRepository> : ITicketStore
        where TSession : TicketStoreSession<TUser>, new()
        where TRepository : IRepository<TSession>
        where TUser : class
    {
        private readonly IServiceProvider _serviceProvider;

        public RepositoryTicketStore(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetService<TRepository>();
            if (repository == null) throw new InvalidOperationException();

            var manager = scope.ServiceProvider.GetService<UserManager<TUser>>();
            if (manager == null) throw new InvalidOperationException();

            var now = DateTime.UtcNow;

            var entity = new TSession
            {
                User = await manager.GetUserAsync(ticket.Principal).ConfigureAwait(false),
                Type = ticket.AuthenticationScheme,
                Value = SerializeToBytes(ticket),
                ExpiresAt = ticket.Properties.ExpiresUtc?.UtcDateTime,
                CreatedAt = ticket.Properties.IssuedUtc?.UtcDateTime ?? now,
                UpdatedAt = now
            };

            await repository.AddAsync(entity).ConfigureAwait(false);
            await repository.CommitAsync().ConfigureAwait(false);

            return entity.Id.ToString(CultureInfo.InvariantCulture);
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            if (!long.TryParse(key, out var id)) return;

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetService<TRepository>();
            if (repository == null) throw new InvalidOperationException();

            var entity = await repository.GetByIdAsync(id).ConfigureAwait(false);
            if (entity == null) return;

            entity.Value = SerializeToBytes(ticket);
            entity.ExpiresAt = ticket.Properties.ExpiresUtc?.UtcDateTime;
            entity.UpdatedAt = DateTime.UtcNow;

            await repository.UpdateAsync(entity).ConfigureAwait(false);
            await repository.CommitAsync().ConfigureAwait(false);
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            if (!long.TryParse(key, out var id)) return null;

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetService<TRepository>();
            if (repository == null) throw new InvalidOperationException();

            var entity = await repository.GetByIdAsync(id).ConfigureAwait(false);
            if (entity == null) return null;

            entity.UpdatedAt = DateTime.UtcNow;

            await repository.UpdateAsync(entity).ConfigureAwait(false);
            await repository.CommitAsync().ConfigureAwait(false);

            var ticket = DeserializeFromBytes(entity.Value);

            ticket.Properties.ExpiresUtc = entity.ExpiresAt != null
                ? DateTime.SpecifyKind(entity.ExpiresAt.Value, DateTimeKind.Utc)
                : null;
            ticket.Properties.IssuedUtc = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);

            return ticket;
        }

        public async Task RemoveAsync(string key)
        {
            if (!long.TryParse(key, out var id)) return;

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetService<TRepository>();
            if (repository == null) throw new InvalidOperationException();

            var entity = await repository.GetByIdAsync(id).ConfigureAwait(false);
            if (entity == null) return;

            await repository.RemoveAsync(entity).ConfigureAwait(false);
            await repository.CommitAsync().ConfigureAwait(false);
        }

        private static byte[] SerializeToBytes(AuthenticationTicket source)
        {
            return TicketSerializer.Default.Serialize(source);
        }

        private static AuthenticationTicket DeserializeFromBytes(byte[] source)
        {
            return source == null ? null : TicketSerializer.Default.Deserialize(source);
        }
    }
}
#nullable restore
