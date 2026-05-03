using AutoMapper;
using BusinessLogicLayer.Dtos.ProjectModule;
using DataAccessLayer.Entities.ProjectModule;

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
            .ForMember(d => d.TotalArea, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Area))));

        // Mapping for Public Details
        CreateMap<Project, ProjectDetailsDto>()
            .ForMember(d => d.BuildingsCount, o => o.MapFrom(s => s.Buildings.Count))
            .ForMember(d => d.UnitsCount, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Count)))
            .ForMember(d => d.TotalArea, o => o.MapFrom(s => s.Buildings.Sum(b => b.Units.Sum(u => u.Area))))
            .ForMember(d => d.Features, o => o.MapFrom(s => s.ProjectFeatures.Select(pf => pf.Feature.Name)))
            .ForMember(d => d.Media, o => o.MapFrom(s => s.Media));

        CreateMap<ProjectMedia, ProjectMediaDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
    }
}
