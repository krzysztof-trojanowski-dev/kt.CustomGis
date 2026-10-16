namespace kt.CustomGis.Core;

public class Poi
{
    public required string OsmType { get; set; }
    public long OsmId { get; set; }
    public string? Name { get; set; }
    public required string Category { get; set; }
    public required string SubType { get; set; }
    public string? Tags { get; set; }
    public double Lon { get;set; }
    public double Lat { get;set; }
}