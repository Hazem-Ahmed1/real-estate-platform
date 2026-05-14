using APILayer.Dtos.Messages;
using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.CommunicationModule;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers;

public class MessagesController(IMessageService messageService) : ApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageCreatedDto>> CreateMessage([FromBody] MessageCreateRequestDto request)
    {
        var dto = new MessageCreateDto(
            request.Type,
            request.FullName,
            request.Email,
            request.Phone,
            request.Subject,
            request.MessageBody
        );

        var created = await messageService.CreateMessageAsync(dto);
        return Ok(created);
    }
}
