namespace BusinessLogicLayer.Dtos.BlogModule;

public sealed record BlogDetailsDto(
    int BlogId,
    string Title,
    DateTime PublishDate,
    string? Description,
    IReadOnlyList<BlogImageDto> Images
);

public sealed record BlogImageDto(
    int ImageId,
    string ImageUrl,
    bool IsThumbnail
);
