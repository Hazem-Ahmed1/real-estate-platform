namespace BusinessLogicLayer.Dtos.BlogModule;

public sealed record BlogListDto(
    int BlogId,
    string Title,
    DateTime PublishDate,
    string? Description,
    BlogImageItemDto? Thumbnail,
    IReadOnlyList<BlogImageItemDto> Images
);

public sealed record BlogImageItemDto(
    int ImageId,
    string ImageUrl
);
