using BusinessLogicLayer.Contracts;
using BusinessLogicLayer.Dtos.CommunicationModule;
using BusinessLogicLayer.Specifications.Messages;
using DataAccessLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers.Admin;

[ApiController]
[Route("api/admin/messages")]
[Authorize(Roles = "Admin")]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageListDto>>> GetMessages([FromQuery] MessageSpecParams @params)
    {
        var result = await messageService.GetMessagesAsync(@params);
        return Ok(result);
    }

    [HttpGet("{messageId:int}")]
    public async Task<ActionResult<MessageDetailsDto>> GetMessage(int messageId)
    {
        var message = await messageService.GetMessageByIdAsync(messageId);
        return Ok(message);
    }

    [HttpDelete("{messageId:int}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        await messageService.DeleteMessageAsync(messageId);
        return NoContent();
    }
}
