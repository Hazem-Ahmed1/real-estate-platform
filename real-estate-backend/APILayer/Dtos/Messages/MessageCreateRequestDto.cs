using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Enums;

namespace APILayer.Dtos.Messages;

public sealed class MessageCreateRequestDto
{
    public MessageType Type { get; set; } = MessageType.GeneralContact;
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Subject { get; set; }

    [Required]
    public string MessageBody { get; set; } = string.Empty;
}
