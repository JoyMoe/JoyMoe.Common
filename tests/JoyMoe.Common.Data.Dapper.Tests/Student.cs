using System;

namespace JoyMoe.Common.Data.Dapper.Tests
{
    public record Student : IDataEntity, ISoftDelete
    {
        public long Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
