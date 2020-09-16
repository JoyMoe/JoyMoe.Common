using System;

namespace JoyMoe.Common.EntityFrameworkCore.Models
{
    public interface ITimestamp
    {
        DateTimeOffset CreatedAt { get; set; }

        DateTimeOffset UpdatedAt { get; set; }
    }
}
