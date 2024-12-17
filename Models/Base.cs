namespace WebApi.Models;

public class Pagination<T>
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<T> Items { get; set; }

    public Pagination(List<T> items, int count, int page, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = page;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }
}