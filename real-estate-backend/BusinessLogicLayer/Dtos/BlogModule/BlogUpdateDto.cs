namespace BusinessLogicLayer.Dtos.BlogModule;

public sealed record BlogUpdateDto(
    string? Title,
    string? Description,
    DateTime? PublishDate,
    IReadOnlyList<BlogImageCreateDto>? Images
);
