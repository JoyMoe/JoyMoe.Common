using System.Collections.Generic;

namespace JoyMoe.Common.Abstractions
{
    public class PaginationResponse<TKey, T> where TKey : struct
    {
        public TKey? Prev { get; set; }

        public TKey? Next { get; set; }

        public ICollection<T>? Data { get; set; }
    }
}
