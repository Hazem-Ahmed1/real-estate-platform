namespace BusinessLogicLayer.Mapping;

public class BuildingProfile : Profile
{
    public BuildingProfile()
    {
        CreateMap<Building, BuildingDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project.Name));
    }
}
