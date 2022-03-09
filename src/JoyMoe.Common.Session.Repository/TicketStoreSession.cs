using System;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Session.Repository;

public class TicketStoreSession<TUser> : ITimestamp where TUser : class
{
    public virtual Guid Id { get; set; }

    public virtual TUser? User { get; set; }

    public virtual string Type { get; set; } = null!;

#pragma warning disable CA1819 // Properties should not return arrays
    public byte[] Value { get; set; } = null!;
#pragma warning restore CA1819 // Properties should not return arrays

    public DateTimeOffset? ExpirationDate { get; set; }

    public DateTimeOffset? CreationDate { get; set; }

    public DateTimeOffset? ModificationDate { get; set; }

    public TicketStoreSession() {
        Id = Guid.NewGuid();
    }
}
