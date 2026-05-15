using DataAccessLayer.Enums;

namespace BusinessLogicLayer.Dtos.CommunicationModule;

public sealed record MessageDetailsDto(
    int MessageId,
    MessageType Type,
    string FullName,
    string? Email,
    string? Phone,
    string? Subject,
    string MessageBody,
    DateTime CreatedAt
);
