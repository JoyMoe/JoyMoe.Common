using System;
using JoyMoe.Common.EntityFrameworkCore.Models;

namespace JoyMoe.Common.Session.EntityFrameworkCore
{
    public class EntityTicketStoreSession<TUser> : ITimestamp where TUser : class
    {
        public virtual Guid Id { get; set; }

        public virtual TUser? User { get; set; }

        public virtual string Type { get; set; } = null!;

#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Value { get; set; } = null!;
#pragma warning restore CA1819 // Properties should not return arrays

        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
