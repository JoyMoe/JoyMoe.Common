using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace JoyMoe.Common.Session
{
    /// <summary>
    /// This provides an storage mechanic to preserve identity information on the <see cref="IDistributedCache" /> while only sending a simple identifier key to the client.
    /// </summary>
    public class CachedTicketStore : ITicketStore
    {
        private const string KeyPrefix = "AuthSessionStore-";
        private readonly IDistributedCache _cache;

        public CachedTicketStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var guid = Guid.NewGuid();
            var key = KeyPrefix + guid;
            await RenewAsync(key, ticket).ConfigureAwait(false);
            return key;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            var options = new DistributedCacheEntryOptions();
            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                options.SetAbsoluteExpiration(expiresUtc.Value);
            }
            var val = SerializeToBytes(ticket);
            _cache.Set(key, val, options);
            return Task.FromResult(0);
        }

        public Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            var bytes = _cache.Get(key);
            var ticket = DeserializeFromBytes(bytes);
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.FromResult(0);
        }

        private static byte[] SerializeToBytes(AuthenticationTicket source)
        {
            return TicketSerializer.Default.Serialize(source);
        }

        private static AuthenticationTicket? DeserializeFromBytes(byte[]? source)
        {
            return source == null ? null : TicketSerializer.Default.Deserialize(source);
        }
    }
}
