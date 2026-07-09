using System.Collections.Generic;

namespace Maham.Application.DTOs.Common;

public class PagedResponseDto<T>
{
    public IEnumerable<T> Items { get; set; } = null!;
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
