using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace JoyMoe.Common.Session;

public class SessionStoreOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _store;

    public SessionStoreOptions(ITicketStore store)
    {
        _store = store;
    }

    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.SessionStore = _store;
    }
}
