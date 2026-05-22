using BusinessLogicLayer.Dtos.LookupModule;

namespace BusinessLogicLayer.Contracts;

public interface IFilterOptionsService
{
    Task<SearchFilterOptionsDto> GetSearchFilterOptionsAsync();
}
