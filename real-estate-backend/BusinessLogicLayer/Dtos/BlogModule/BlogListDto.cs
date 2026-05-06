namespace BusinessLogicLayer.Dtos.BlogModule;

public sealed record BlogListDto(
    int BlogId,
    string Title,
    DateTime PublishDate,
    string? Description,
    string? ThumbnailUrl
);
