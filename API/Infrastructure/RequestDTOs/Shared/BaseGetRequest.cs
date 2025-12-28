using System;

namespace API.Infrastructure.RequestDTOs.Shared;

public class BaseGetRequest<TFilter>
{
    public PagerRequest Pager { get; set; }
    public string OrderBy { get; set; }
    public bool SortAsc { get; set; }
    public TFilter Filter { get; set; }
}
