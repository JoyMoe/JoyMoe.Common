using System.Collections.Generic;
using System.Linq;
using JoyMoe.Common.EntityFrameworkCore.Models;

namespace JoyMoe.Common.Mvc.Api.ViewModels
{
    public class PaginationResponse<T> where T : IDataEntity
    {
        public long? Before { get; }

        public int Size { get; }

        public long? Last { get; }

        public IList<T>? Data { get; }

        public PaginationResponse(IList<T>? data, long? before = null)
        {
            Before = before;
            Size = data?.Count ?? 0;
            Last = data?.LastOrDefault()?.Id;
            Data = data;
        }
    }
}
