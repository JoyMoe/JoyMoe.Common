using System.Collections.Generic;

namespace JoyMoe.Common.Abstractions;

public class CursorPaginationResponse<TKey, T> where TKey : struct
{
    public TKey? Next { get; set; }

    public ICollection<T>? Data { get; set; }
}
