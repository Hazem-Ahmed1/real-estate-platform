using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.CommunicationModule;
using BusinessLogicLayer.Exceptions;
using BusinessLogicLayer.Specifications.Messages;
using DataAccessLayer.Common;
using DataAccessLayer.Contracts;
using DataAccessLayer.Entities.CommunicationModule;

namespace BusinessLogicLayer.Implementation;

public class MessageService(IUnitOfWork unitOfWork) : IMessageService
{
    public async Task<MessageCreatedDto> CreateMessageAsync(MessageCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            throw new BadRequestException("FullName is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.MessageBody))
        {
            throw new BadRequestException("MessageBody is required.");
        }

        var message = new Message
        {
            Type = dto.Type,
            FullName = dto.FullName.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim(),
            MessageBody = dto.MessageBody.Trim()
        };

        await unitOfWork.Repository<Message>().AddAsync(message);
        await unitOfWork.CompleteAsync();

        return new MessageCreatedDto(message.MessageId, message.CreatedAt);
    }

    public async Task<PaginatedResult<MessageListDto>> GetMessagesAsync(MessageSpecParams @params)
    {
        var spec = new MessagesSpecification(@params);
        var countSpec = new MessagesSpecification(@params, isCount: true);

        var totalItems = await unitOfWork.Repository<Message>().CountAsync(countSpec);
        var messages = await unitOfWork.Repository<Message>().GetAllAsync(spec);

        var data = messages
            .Select(m => new MessageListDto(
                m.MessageId,
                m.Type,
                m.FullName,
                m.Subject,
                m.CreatedAt
            ))
            .ToList();

        return new PaginatedResult<MessageListDto>(@params.Page, @params.PageSize, totalItems, data);
    }

    public async Task<MessageDetailsDto> GetMessageByIdAsync(int id)
    {
        var spec = new MessagesSpecification(id);
        var message = await unitOfWork.Repository<Message>().GetByIdAsync(spec);

        if (message is null)
        {
            throw new NotFoundExpection(nameof(Message), id);
        }

        return new MessageDetailsDto(
            message.MessageId,
            message.Type,
            message.FullName,
            message.Email,
            message.Phone,
            message.Subject,
            message.MessageBody,
            message.CreatedAt
        );
    }

    public async Task DeleteMessageAsync(int id)
    {
        var spec = new MessagesSpecification(id);
        var message = await unitOfWork.Repository<Message>().GetByIdAsync(spec);

        if (message is null)
        {
            throw new NotFoundExpection(nameof(Message), id);
        }

        unitOfWork.Repository<Message>().Remove(message);
        await unitOfWork.CompleteAsync();
    }
}
