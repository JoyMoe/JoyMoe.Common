using System;

namespace JoyMoe.Common.EntityFrameworkCore.Models
{
    public interface ISoftDelete
    {
        DateTimeOffset? DeletedAt { get; set; }
    }
}
