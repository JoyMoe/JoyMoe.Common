using System;
using System.Collections.Generic;

namespace JoyMoe.Common.Session.EntityFrameworkCore
{
    public class EntityTicketStoreSession<TUser> where TUser : class
    {
        public virtual Guid Id { get; set; }

        public virtual TUser? User { get; set; }

        public IEnumerable<byte> Value { get; set; } = null!;

        public DateTimeOffset? ExpiresAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
