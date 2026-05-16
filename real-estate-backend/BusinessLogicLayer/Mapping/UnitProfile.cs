using BusinessLogicLayer.Helpers;
using DataAccessLayer.Entities.LookupModule;

namespace BusinessLogicLayer.Mapping;

public class UnitProfile : Profile
{
    public UnitProfile()
    {
        CreateMap<Unit, UnitListDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Building.Project.Name))
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building.Name))
            .ForMember(d => d.City, o => o.MapFrom(s => s.Building.Project.City))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Building.Project.Address))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.IsThumbnail) != null 
                ? s.Media.FirstOrDefault(m => m.IsThumbnail)!.MediaUrl 
                : (s.Media.Any() ? s.Media.First().MediaUrl : null)));

        CreateMap<Unit, GetUnitDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Building.Project.Name))
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building.Name))
            .ForMember(d => d.NearbyFacilities, o => o.MapFrom(s => s.NearbyFacilities))
            .ForMember(d => d.Features, o => o.MapFrom(s => s.UnitFeatures.Select(uf => uf.Feature)))
            .ForMember(d => d.Insurance, o => o.MapFrom(s => s.UnitInsurance.Select(ui => ui.Insurance)))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.IsThumbnail).MediaUrl))
            .ForMember(d => d.VideoUrl, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.Type == MediaType.Video).MediaUrl))
            .ForMember(d => d.PanoramaUrl, o => o.MapFrom(s => s.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360).MediaUrl))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Media.Where(m => m.Type == MediaType.Image && !m.IsThumbnail).Select(m => m.MediaUrl)))
            .ForMember(d => d.Designs, o => o.MapFrom(s => s.Media.Where(m => m.Type == MediaType.Design).Select(m => m.MediaUrl)));

        CreateMap<NearbyFacility, NearbyFacilityDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<NearbyFacilityDto, NearbyFacility>()
            .ForMember(d => d.Type, o => o.MapFrom(s => Enum.Parse<FacilityType>(s.Type, true)));

        CreateMap<UnitMedia, UnitMediaDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

            
        CreateMap<UnitCreateDto, Unit>()
            .ForMember(d => d.Media, o => o.Ignore());
        CreateMap<UnitUpdateDto, Unit>()
            .ForMember(d => d.Media, o => o.Ignore());
    }
}
