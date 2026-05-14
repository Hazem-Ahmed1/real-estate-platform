namespace BusinessLogicLayer.Dtos.BlogModule;

public sealed record BlogDetailsDto(
    int BlogId,
    string Title,
    DateTime PublishDate,
    string? Description,
    BlogImageItemDto? Thumbnail,
    IReadOnlyList<BlogImageItemDto> Images
);
