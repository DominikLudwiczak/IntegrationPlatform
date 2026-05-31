using Microsoft.EntityFrameworkCore;

namespace Application.Common.Models;

public class PaginatedListDto<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; set;  }
    public int TotalCount { get; }

    public PaginatedListDto(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public static async Task<PaginatedListDto<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedListDto<T>(items, count, pageNumber, pageSize);
    }
}