using AutoMapper;
using BusinessLogicLayer.Dtos.LookupModule;
using BusinessLogicLayer.Dtos.ProjectModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Enums;

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
            .ForMember(d => d.BuildingsCount, o => o.MapFrom(s => s.Buildings.Count))
            .ForMember(d => d.UnitsCount, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Count)))
            .ForMember(d => d.AvailableUnitsCount, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Count(u => u.Status == UnitStatus.ForSale))))
            .ForMember(d => d.TotalRooms, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Rooms ?? 0))))
            .ForMember(d => d.TotalHalls, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Salons ?? 0))))
            .ForMember(d => d.MinPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Min(u => u.Price ?? 0) : 0))
            .ForMember(d => d.MaxPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Max(u => u.Price ?? 0) : 0))
            .ForMember(d => d.TotalArea, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Area ?? 0))));

        // Mapping for Public Details
        CreateMap<Project, GetProjectDto>()
            .ForMember(d => d.BuildingsCount, o => o.MapFrom(s => s.Buildings.Count))
            .ForMember(d => d.UnitsCount, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Count)))
            .ForMember(d => d.AvailableUnitsCount, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Count(u => u.Status == UnitStatus.ForSale))))
            .ForMember(d => d.TotalRooms, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Rooms ?? 0))))
            .ForMember(d => d.TotalHalls, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Salons ?? 0))))
            .ForMember(d => d.MinPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Min(u => u.Price ?? 0) : 0))
            .ForMember(d => d.MaxPrice, o => o.MapFrom(s => s.Buildings.SelectMany(b => b.Units).Any() ? s.Buildings.SelectMany(b => b.Units).Max(u => u.Price ?? 0) : 0))
            .ForMember(d => d.TotalArea, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Area ?? 0))))
            .ForMember(d => d.Features, o => o.MapFrom(s => s.ProjectFeatures.Select(pf => pf.Feature.Name)))
            .ForMember(d => d.Insurance, o => o.MapFrom(s => s.ProjectInsurance.Select(pi => pi.Insurance.Name)))
            .ForMember(d => d.Panorama360Url, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360) != null 
                ? s.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360)!.MediaUrl 
                : null))
            .ForMember(d => d.Video, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.Type == MediaType.Video) != null 
                ? new ProjectVideoDto { VideoUrl = s.Media.FirstOrDefault(m => m.Type == MediaType.Video)!.MediaUrl, PosterUrl = s.Media.FirstOrDefault(m => m.Type == MediaType.Video)!.ThumbnailUrl } 
                : null))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Media.Where(m => m.Type == MediaType.Image)));

        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();

        CreateMap<ProjectMedia, ProjectMediaDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));


        CreateMap<Feature, FeatureDto>();
        CreateMap<Insurance, InsuranceDto>();
        CreateMap<Building, BuildingDto>();
    }
}
