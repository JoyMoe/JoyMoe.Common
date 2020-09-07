using System;

namespace JoyMoe.Common.EntityFrameworkCore.Models
{
    public interface IDataEntity
    {
        long Id { get; set; }

        DateTimeOffset CreatedAt { get; set; }

        DateTimeOffset UpdatedAt { get; set; }
    }
}
