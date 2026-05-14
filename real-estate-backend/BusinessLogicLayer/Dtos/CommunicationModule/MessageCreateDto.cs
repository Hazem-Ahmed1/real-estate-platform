using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Dtos.CommunicationModule;

public sealed record MessageCreateDto(
    MessageType Type,
    string FullName,
    string? Email,
    string? Phone,
    string? Subject,
    string MessageBody
);

public sealed record MessageCreatedDto(
    int MessageId,
    DateTime CreatedAt
);
