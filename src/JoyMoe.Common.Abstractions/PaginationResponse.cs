using System.Collections.Generic;
using System.Linq;

namespace JoyMoe.Common.Abstractions
{
    public class PaginationResponse<T> where T : IIdentifier
    {
        public long? Before { get; set; }

        public int Size { get; set; }

        public long? Last { get; set; }

        public IEnumerable<T>? Data { get; set; }

        public PaginationResponse() { }

        public PaginationResponse(IList<T>? data, long? before = null)
        {
            Before = before;
            Size = data?.Count ?? 0;
            Last = data?.LastOrDefault()?.Id;
            Data = data;
        }
    }
}
