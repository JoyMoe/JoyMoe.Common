using System;

namespace JoyMoe.Common.Abstractions;

public interface ITimestamp
{
    DateTimeOffset? CreationDate { get; set; }

    DateTimeOffset? ModificationDate { get; set; }
}
