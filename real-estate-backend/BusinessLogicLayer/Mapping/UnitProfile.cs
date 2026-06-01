using BusinessLogicLayer.Helpers;
using DataAccessLayer.Entities.LookupModule;

namespace BusinessLogicLayer.Mapping;

public class UnitProfile : Profile
{
    public UnitProfile()
    {
        CreateMap<Unit, UnitListDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Building != null && s.Building.Project != null ? s.Building.Project.Name : string.Empty))
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building != null ? s.Building.Name : string.Empty))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Media != null && s.Media.FirstOrDefault(m => m.IsThumbnail) != null 
                ? s.Media.FirstOrDefault(m => m.IsThumbnail)!.MediaUrl 
                : (s.Media != null && s.Media.Any() ? s.Media.First().MediaUrl : null)));

        CreateMap<Unit, GetUnitDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Building != null && s.Building.Project != null ? s.Building.Project.Name : string.Empty))
            .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Building != null ? s.Building.Name : string.Empty))
            .ForMember(d => d.NearbyFacilities, o => o.MapFrom(s => s.NearbyFacilities))
            .ForMember(d => d.Features, o => o.MapFrom(s => s.UnitFeatures != null ? s.UnitFeatures.Select(uf => uf.Feature) : null))
            .ForMember(d => d.Insurance, o => o.MapFrom(s => s.UnitInsurance != null ? s.UnitInsurance.Select(ui => ui.Insurance) : null))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.Media != null && s.Media.FirstOrDefault(m => m.IsThumbnail) != null 
                ? s.Media.FirstOrDefault(m => m.IsThumbnail)!.MediaUrl 
                : null))
            .ForMember(d => d.VideoUrl, o => o.MapFrom(s => s.Media != null && s.Media.FirstOrDefault(m => m.Type == MediaType.Video) != null 
                ? s.Media.FirstOrDefault(m => m.Type == MediaType.Video)!.MediaUrl 
                : null))
            .ForMember(d => d.PanoramaUrl, o => o.MapFrom(s => s.Media != null && s.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360) != null 
                ? s.Media.FirstOrDefault(m => m.Type == MediaType.Panorama360)!.MediaUrl 
                : null))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Media != null ? s.Media.Where(m => m.Type == MediaType.Image && !m.IsThumbnail).Select(m => m.MediaUrl) : null))
            .ForMember(d => d.Designs, o => o.MapFrom(s => s.Media != null ? s.Media.Where(m => m.Type == MediaType.Design).Select(m => m.MediaUrl) : null))
            .ForMember(d => d.Media, o => o.MapFrom(s => s.Media));

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
