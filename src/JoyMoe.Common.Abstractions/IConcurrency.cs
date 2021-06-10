using System;

namespace JoyMoe.Common.Abstractions
{
    public interface IConcurrency
    {
        Guid? Timestamp { get; set; }
    }
}
