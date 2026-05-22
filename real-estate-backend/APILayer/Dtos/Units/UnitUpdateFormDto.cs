namespace APILayer.Dtos.Units;

public class UnitUpdateFormDto : UnitCreateFormDto
{
    public List<int> DeletedMediaIds { get; set; } = new();
}
