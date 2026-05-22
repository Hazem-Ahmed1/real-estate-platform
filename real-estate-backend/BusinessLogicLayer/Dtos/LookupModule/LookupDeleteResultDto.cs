namespace BusinessLogicLayer.Dtos.LookupModule;

public class LookupDeleteResultDto<TDto>
{
    public string Message { get; set; } = string.Empty;

    public TDto Item { get; set; } = default!;
}
