using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.LookupModule;
using DataAccessLayer.Data;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Implementation;

public class FilterOptionsService(RealEstateDbContext context) : IFilterOptionsService
{
    public async Task<SearchFilterOptionsDto> GetSearchFilterOptionsAsync()
    {
        var cities = await context.Projects.AsNoTracking()
            .Select(p => p.City)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .GroupBy(c => c)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.City)
            .Take(3)
            .Select(x => x.City)
            .ToListAsync();

        return new SearchFilterOptionsDto
        {
            Cities = cities
        };
    }
}
