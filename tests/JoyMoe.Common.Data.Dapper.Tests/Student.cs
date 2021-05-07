using System;

namespace JoyMoe.Common.Data.Dapper.Tests
{
    public record Person : IDataEntity, IConcurrency, ISoftDelete
    {
        public virtual long Id { get; set; }

        public virtual string FirstName { get; set; } = null!;

        public virtual string LastName { get; set; } = null!;

        public virtual Guid Timestamp { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual DateTime ModificationDate { get; set; }

        public virtual DateTime? DeletionDate { get; set; }
    }

    public record Student : Person
    {
        public virtual int Grade { get; set; }
    }
}
