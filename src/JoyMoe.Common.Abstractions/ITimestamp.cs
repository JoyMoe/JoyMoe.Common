using System;

namespace JoyMoe.Common.Abstractions;

public interface ITimestamp
{
    DateTime? CreationDate { get; set; }

    DateTime? ModificationDate { get; set; }
}
