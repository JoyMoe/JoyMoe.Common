using System;

namespace JoyMoe.Common.EntityFrameworkCore.Models
{
    public interface ISoftDelete
    {
        DateTime? DeletedAt { get; set; }
    }
}
