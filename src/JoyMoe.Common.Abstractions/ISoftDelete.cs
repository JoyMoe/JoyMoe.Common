using System;

namespace JoyMoe.Common.Abstractions;

public interface ISoftDelete
{
    DateTimeOffset? DeletionDate { get; set; }
}
