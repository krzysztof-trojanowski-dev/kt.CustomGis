namespace kt.CustomGis.Core;

public class PageRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public int Offset => PageSize * (PageNumber - 1);
}