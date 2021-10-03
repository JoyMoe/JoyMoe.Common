using System;
using JoyMoe.Common.Data;
using JoyMoe.Common.Session;
using JoyMoe.Common.Session.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class RepositoryTicketStoreServiceCollectionExtensions
{
    /// <summary>
    /// Add a <see cref="RepositoryTicketStore{TUser,AspNetSession, IRepository}"/> to preserve identity information
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRepositoryTicketStore<TUser>(this IServiceCollection services)
        where TUser : class
    {
        return services.AddRepositoryTicketStore<TUser, TicketStoreSession<TUser>>();
    }

    /// <summary>
    /// Add a <see cref="RepositoryTicketStore{TUser, TSession, IRepository}"/> to preserve identity information
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRepositoryTicketStore<TUser, TSession>(this IServiceCollection services)
        where TSession : TicketStoreSession<TUser>, new()
        where TUser : class
    {
        return services.AddRepositoryTicketStore<TUser, TSession, IRepository<TSession>>();
    }

    /// <summary>
    /// Add a <see cref="RepositoryTicketStore{TUser, TSession, TRepository}"/> to preserve identity information
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRepositoryTicketStore<TUser, TSession, TRepository>(
        this IServiceCollection services)
        where TSession : TicketStoreSession<TUser>, new()
        where TRepository : IRepository<TSession>
        where TUser : class
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddSingleton<ITicketStore, RepositoryTicketStore<TUser, TSession, TRepository>>();
        services.TryAddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, SessionStoreOptions>();

        return services;
    }
}
