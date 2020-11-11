using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

#nullable disable
namespace JoyMoe.Common.Session.EntityFrameworkCore
{
    /// <summary>
    /// This provides an storage mechanic to preserve identity information in the EntityFrameworkCore while only sending a simple identifier key to the client.
    /// </summary>
    public class EntityTicketStore<TContext, TUser, TSession> : ITicketStore
        where TContext : DbContext
        where TSession : EntityTicketStoreSession<TUser>
        where TUser : class
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityTicketStore(IServiceProvider serviceProvider)
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

            var context = scope.ServiceProvider.GetService<TContext>();
            if (context == null) throw new InvalidOperationException();

            var manager = scope.ServiceProvider.GetService<UserManager<TUser>>();
            if (manager == null) throw new InvalidOperationException();

            var now = DateTime.UtcNow;

            var entity = new EntityTicketStoreSession<TUser>
            {
                User = await manager.GetUserAsync(ticket.Principal).ConfigureAwait(false),
                Type = ticket.AuthenticationScheme,
                Value = SerializeToBytes(ticket),
                ExpiresAt = ticket.Properties.ExpiresUtc?.UtcDateTime,
                CreatedAt = ticket.Properties.IssuedUtc?.UtcDateTime ?? now,
                UpdatedAt = now
            };

            context.Add(entity);

            await context.SaveChangesAsync().ConfigureAwait(false);

            return entity.Id.ToString();
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            if (!Guid.TryParse(key, out var guid)) return;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<TContext>();
            if (context == null) throw new InvalidOperationException();

            var entity = await context.Set<TSession>().FindAsync(guid).ConfigureAwait(false);
            if (entity == null) return;

            entity.Value = SerializeToBytes(ticket);
            entity.ExpiresAt = ticket.Properties.ExpiresUtc?.UtcDateTime;
            entity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            if (!Guid.TryParse(key, out var guid)) return null;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<TContext>();
            if (context == null) throw new InvalidOperationException();

            var entity = await context.Set<TSession>().FindAsync(guid).ConfigureAwait(false);
            if (entity == null) return null;

            entity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync().ConfigureAwait(false);

            var ticket = DeserializeFromBytes(entity.Value.ToArray());

            ticket.Properties.ExpiresUtc = entity.ExpiresAt != null
                ? DateTime.SpecifyKind(entity.ExpiresAt.Value, DateTimeKind.Utc)
                : null;
            ticket.Properties.IssuedUtc = DateTime.SpecifyKind(entity.CreatedAt, DateTimeKind.Utc);

            return ticket;
        }

        public async Task RemoveAsync(string key)
        {
            if (!Guid.TryParse(key, out var guid)) return;

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<TContext>();
            if (context == null) throw new InvalidOperationException();

            var entity = await context.Set<TSession>().FindAsync(guid).ConfigureAwait(false);
            if (entity == null) return;

            context.Remove(entity);

            await context.SaveChangesAsync().ConfigureAwait(false);
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
