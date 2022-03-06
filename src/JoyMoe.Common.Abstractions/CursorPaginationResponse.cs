namespace JoyMoe.Common.Abstractions;

public class CursorPaginationResponse<TKey, T> : PaginationResponseBase<T> where TKey : struct
{
    public TKey? Next { get; set; }
}
