using System;

namespace API.Infrastructure.RequestDTOs.Shared;

public class PagerRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}
