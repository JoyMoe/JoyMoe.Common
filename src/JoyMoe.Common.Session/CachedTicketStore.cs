using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace JoyMoe.Common.Session;

/// <summary>
/// This provides an storage mechanic to preserve identity information on the <see cref="IDistributedCache" /> while only sending a simple identifier key to the client.
/// </summary>
public class CachedTicketStore : ITicketStore
{
    private const    string            KeyPrefix = "AuthSessionStore-";
    private readonly IDistributedCache _cache;

    public CachedTicketStore(IDistributedCache cache) {
        _cache = cache;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket) {
        var guid = Guid.NewGuid();
        var key  = KeyPrefix + guid;
        await RenewAsync(key, ticket);
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket) {
        var options    = new DistributedCacheEntryOptions();
        var expiresUtc = ticket.Properties.ExpiresUtc;
        if (expiresUtc.HasValue) options.SetAbsoluteExpiration(expiresUtc.Value);

        var val = SerializeToBytes(ticket);
        await _cache.SetAsync(key, val, options);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key) {
        var bytes  = await _cache.GetAsync(key);
        var ticket = DeserializeFromBytes(bytes);
        return ticket;
    }

    public async Task RemoveAsync(string key) {
        await _cache.RemoveAsync(key);
    }

    private static byte[] SerializeToBytes(AuthenticationTicket source) {
        return TicketSerializer.Default.Serialize(source);
    }

    private static AuthenticationTicket? DeserializeFromBytes(byte[]? source) {
        return source == null ? null : TicketSerializer.Default.Deserialize(source);
    }
}
