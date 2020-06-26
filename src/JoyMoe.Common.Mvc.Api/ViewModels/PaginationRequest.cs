using System.ComponentModel.DataAnnotations;

namespace JoyMoe.Common.Mvc.Api.ViewModels
{
    public class PaginationRequest
    {
        public long? Before { get; set; }

        [Range(0, 30)]
        public int Size { get; set; } = 30;
    }
}
