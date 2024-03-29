using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Session;

public class SessionStoreOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _store;

    public SessionStoreOptions(ITicketStore store) {
        _store = store;
    }

    public void PostConfigure(string name, CookieAuthenticationOptions options) {
        options.SessionStore = _store;
    }
}
