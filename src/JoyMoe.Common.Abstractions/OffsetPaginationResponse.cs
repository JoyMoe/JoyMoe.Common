namespace JoyMoe.Common.Abstractions;

public class OffsetPaginationResponse<T> : PaginationResponseBase<T>
{
    public int Size { get; set; }
    public int Page { get; set; }
}
