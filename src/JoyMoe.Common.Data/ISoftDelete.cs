using System;

namespace JoyMoe.Common.Data
{
    public interface ISoftDelete
    {
        DateTime? DeletedAt { get; set; }
    }
}
