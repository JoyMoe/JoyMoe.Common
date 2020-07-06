using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JoyMoe.Common.EntityFrameworkCore.Models;

namespace JoyMoe.Common.Mvc.Api.ViewModels
{
    public class PaginationResponse<T> where T : IIdentifier
    {
        public long? Before { get; set; }

        public int Size { get; set; }

        public long? Last { get; set; }

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
