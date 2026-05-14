using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Dtos.CommunicationModule;

public sealed record MessageListDto(
    int MessageId,
    MessageType Type,
    string FullName,
    string? Subject,
    DateTime CreatedAt
);
