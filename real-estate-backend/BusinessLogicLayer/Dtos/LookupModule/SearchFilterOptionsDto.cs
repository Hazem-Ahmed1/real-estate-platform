namespace BusinessLogicLayer.Dtos.LookupModule;

public class SearchFilterOptionsDto
{
    public IReadOnlyList<string> Cities { get; init; } = Array.Empty<string>();
}
