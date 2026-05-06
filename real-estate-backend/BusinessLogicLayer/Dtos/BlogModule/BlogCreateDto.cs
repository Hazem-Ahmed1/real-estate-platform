namespace BusinessLogicLayer.Dtos.BlogModule;

public sealed record BlogCreateDto(
    string Title,
    string? Description,
    DateTime? PublishDate,
    IReadOnlyList<BlogImageCreateDto>? Images
);

public sealed record BlogImageCreateDto(
    string ImageUrl,
    bool IsThumbnail
);
