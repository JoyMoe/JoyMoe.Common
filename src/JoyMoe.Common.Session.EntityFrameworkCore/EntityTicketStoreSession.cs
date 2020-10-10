using System;
using System.Collections.Generic;
using JoyMoe.Common.EntityFrameworkCore.Models;

namespace JoyMoe.Common.Session.EntityFrameworkCore
{
    public class EntityTicketStoreSession<TUser> : ITimestamp where TUser : class
    {
        public virtual Guid Id { get; set; }

        public virtual TUser? User { get; set; }

        public virtual string Type { get; set; } = null!;

        public IEnumerable<byte> Value { get; set; } = null!;

        public DateTimeOffset? ExpiresAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
