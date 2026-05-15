namespace BusinessLogicLayer.Mapping;

public class BuildingProfile : Profile
{
    public BuildingProfile()
    {
        CreateMap<Building, BuildingDto>();
    }
}
