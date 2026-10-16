namespace kt.CustomGis.Core;

public interface IPoiRepository
{
    Task<PagedResult<Poi>> GetPoiAsync(PageRequest pageRequest);
    Task<PagedResult<Poi>> GetPoiFilteredAsync(PoiFilteredRequests poiFilteredRequests);
}