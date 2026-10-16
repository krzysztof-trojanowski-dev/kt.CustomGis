using Dapper;
using kt.CustomGis.Core;
using Npgsql;

namespace kt.CustomGis.Data;

public class PoiRepository : IPoiRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PoiRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }
    
    public async Task<PagedResult<Poi>> GetPoiAsync(PageRequest pageRequest)
    {
        const string sql = @"
            SELECT 
                osm_type, 
                osm_id, 
                name, 
                category, 
                subtype, 
                ST_X(geom) AS Lon, 
                ST_Y(geom) AS Lat
            FROM poi
            ORDER BY osm_id, osm_type
            LIMIT @PageSize OFFSET @Offset;";
        const string countSql = "SELECT COUNT(*) FROM poi;";
        var result = new PagedResult<Poi>
        {
            PageSize = pageRequest.PageSize,
            PageNumber = pageRequest.PageNumber
        };
        await using var connection = await _dataSource.OpenConnectionAsync();
        result.TotalCount = await connection.ExecuteScalarAsync<long>(countSql);
        result.Items = (await connection.QueryAsync<Poi>(sql, new
        {
            pageRequest.PageSize,
            pageRequest.Offset
        })).ToList();
        return result;
    }

    public async Task<PagedResult<Poi>> GetPoiFilteredAsync(PoiFilteredRequests poiFilteredRequests)
    {
        const string sql = @"
            SELECT 
                osm_type, 
                osm_id, 
                name, 
                category, 
                subtype, 
                ST_X(geom) AS Lon, 
                ST_Y(geom) AS Lat
            FROM poi
            WHERE geom && ST_MakeEnvelope(@minLon, @minLat, @maxLon, @maxLat, 4326)
            ORDER BY osm_id, osm_type
            LIMIT @PageSize OFFSET @Offset;";
        const string countSql = "SELECT COUNT(*) FROM poi WHERE geom && ST_MakeEnvelope(@minLon, @minLat, @maxLon, @maxLat, 4326);";
        var result = new PagedResult<Poi>
        {
            PageSize = poiFilteredRequests.PageSize,
            PageNumber = poiFilteredRequests.PageNumber
        };
        await using var connection = await _dataSource.OpenConnectionAsync();
        result.TotalCount = await connection.ExecuteScalarAsync<long>(countSql, new
        {
            poiFilteredRequests.MinLon,
            poiFilteredRequests.MinLat,
            poiFilteredRequests.MaxLon,
            poiFilteredRequests.MaxLat
        });
        result.Items = (await connection.QueryAsync<Poi>(sql, new
        {
            poiFilteredRequests.PageSize,
            poiFilteredRequests.Offset,
            poiFilteredRequests.MinLon,
            poiFilteredRequests.MinLat,
            poiFilteredRequests.MaxLon,
            poiFilteredRequests.MaxLat
        })).ToList();
        return result;
    }
}