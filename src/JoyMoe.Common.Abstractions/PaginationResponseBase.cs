using System.Collections.Generic;

namespace JoyMoe.Common.Abstractions;

public abstract class PaginationResponseBase<T>
{
    public ICollection<T>? Data { get; set; }
}
