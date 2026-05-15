
namespace BusinessLogicLayer.Mapping
{
    internal class LookupProfile : Profile
    {
        public LookupProfile()
        {
            CreateMap<Feature, FeatureDto>();
            CreateMap<Insurance, InsuranceDto>();
        }
    }
}
