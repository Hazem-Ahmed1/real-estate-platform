namespace BusinessLogicLayer.Dtos.UnitModule;

public class UnitUpdateDto : UnitCreateDto
{
    public List<int> DeletedMediaIds { get; set; } = new();
}
