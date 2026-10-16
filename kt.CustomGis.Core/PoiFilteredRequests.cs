namespace kt.CustomGis.Core;

public class PoiFilteredRequests : PageRequest
{
    public double MinLon { get;set; }
    public double MinLat { get;set; }
    public double MaxLon { get;set; }
    public double MaxLat { get;set; }
}