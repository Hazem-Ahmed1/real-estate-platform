using BusinessLogicLayer.Dtos.CommunicationModule;
using BusinessLogicLayer.Specifications.Messages;
using DataAccessLayer.Common;

namespace BusinessLogicLayer.Contracts;

public interface IMessageService
{
    Task<MessageCreatedDto> CreateMessageAsync(MessageCreateDto dto);
    Task<PaginatedResult<MessageListDto>> GetMessagesAsync(MessageSpecParams @params);
    Task<MessageDetailsDto> GetMessageByIdAsync(int id);
    Task DeleteMessageAsync(int id);
}
