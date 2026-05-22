namespace BusinessLogicLayer.Mapping;

public class BuildingProfile : Profile
{
    public BuildingProfile()
    {
        CreateMap<Building, BuildingDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project != null ? s.Project.Name : string.Empty))
            .ForMember(d => d.Status,      o => o.MapFrom(s => s.Status))
            .ForMember(d => d.Type,        o => o.MapFrom(s => s.Type))
            .ForMember(d=> d.FloorCount, o => o.MapFrom(s => s.FloorCount))
            .ForMember(d => d.Units,       o => o.MapFrom(s => s.Units));
    }
}
