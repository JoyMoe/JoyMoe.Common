using System;
using System.ComponentModel.DataAnnotations;

namespace JoyMoe.Common.Data
{
    public interface IConcurrency
    {
        [Timestamp]
        Guid? Timestamp { get; set; }
    }
}
