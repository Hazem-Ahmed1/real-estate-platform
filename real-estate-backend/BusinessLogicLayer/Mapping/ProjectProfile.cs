
namespace BusinessLogicLayer.Mapping;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        // Mapping for Public Listing
        CreateMap<Project, ProjectListDto>()
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Media != null && s.Media.Any(m => m.IsThumbnail)
                ? s.Media.FirstOrDefault(m => m.IsThumbnail)!.MediaUrl
                : null))
            .ForMember(d => d.BuildingsNumber, o => o.MapFrom(s => s.Buildings.Count))
            .ForMember(d => d.UnitsNumber, o => o.MapFrom(s => s.AvailableUnitsCount + s.TransactedUnitsCount))
            .ForMember(d => d.TotalRooms, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Rooms))))
            .ForMember(d => d.TotalHalls, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Salons))))
            .ForMember(d => d.MinPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Min(u => u.Price) : 0))
            .ForMember(d => d.MaxPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Max(u => u.Price) : 0));


        // Mapping for Public Details
        CreateMap<Project, ProjectDetailsDto>()
            .ForMember(d => d.BuildingsNumber, o => o.MapFrom(s => s.Buildings.Count))
            .ForMember(d => d.UnitsNumber, o => o.MapFrom(s => s.AvailableUnitsCount + s.TransactedUnitsCount))
            .ForMember(d => d.TotalRooms, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Rooms))))
            .ForMember(d => d.TotalHalls, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Salons))))
            .ForMember(d => d.MinPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Min(u => u.Price) : 0))
            .ForMember(d => d.MaxPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Max(u => u.Price) : 0))
            .ForMember(d => d.Buildings, o => o.MapFrom(s => s.Buildings))

            .ForMember(d => d.CityName, o => o.MapFrom(s => s.City))
            .ForMember(d => d.AreaName, o => o.MapFrom(s => s.Area))
            .ForMember(d => d.Features, o => o.MapFrom(s => s.ProjectFeatures.Select(pf => pf.Feature)))
            .ForMember(d => d.Insurance, o => o.MapFrom(s => s.ProjectInsurance.Select(pi => pi.Insurance)))
            .ForMember(d => d.Panorama360Url, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360).MediaUrl))
            .ForMember(d => d.VideoUrl, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.Type == MediaType.Video).MediaUrl))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.IsThumbnail).MediaUrl))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Media.Where(m => m.Type == MediaType.Image && !m.IsThumbnail).Select(m => m.MediaUrl)));

        CreateMap<ProjectCreateDto, Project>()
            .ForMember(d => d.Media, o => o.Ignore());
        CreateMap<ProjectUpdateDto, Project>()
            .ForMember(d => d.Media, o => o.Ignore());

        CreateMap<NearbyFacility, NearbyFacilityDto>().ReverseMap();

        CreateMap<ProjectMedia, ProjectMediaDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

    }
}
