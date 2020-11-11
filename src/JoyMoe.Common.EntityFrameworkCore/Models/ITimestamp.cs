using System;

namespace JoyMoe.Common.EntityFrameworkCore.Models
{
    public interface ITimestamp
    {
        DateTime CreatedAt { get; set; }

        DateTime UpdatedAt { get; set; }
    }
}
