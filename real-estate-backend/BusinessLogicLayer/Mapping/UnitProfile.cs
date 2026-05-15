
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
            .ForMember(d => d.Insurance, o => o.MapFrom(s => s.UnitInsurance.Select(ui => ui.Insurance)));

        CreateMap<NearbyFacility, NearbyFacilityDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<UnitMedia, UnitMediaDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

            
        CreateMap<CreateUnitDto, Unit>();
        CreateMap<UpdateUnitDto, Unit>();
    }
}
