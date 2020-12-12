using System;

namespace JoyMoe.Common.Data
{
    public interface ITimestamp
    {
        DateTime CreatedAt { get; set; }

        DateTime UpdatedAt { get; set; }
    }
}
