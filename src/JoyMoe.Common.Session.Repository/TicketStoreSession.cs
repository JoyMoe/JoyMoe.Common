using System;
using JoyMoe.Common.Data;

namespace JoyMoe.Common.Session.Repository
{
    public class TicketStoreSession<TUser> : IDataEntity where TUser : class
    {
        public virtual long Id { get; set; }

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
