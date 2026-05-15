using BusinessLogicLayer.Specifications;
using DataAccessLayer.Entities.CommunicationModule;

namespace BusinessLogicLayer.Specifications.Messages;

public sealed class MessagesSpecification : BaseSpecifications<Message>
{
    public MessagesSpecification(MessageSpecParams @params, bool isCount = false)
        : base(m =>
            string.IsNullOrWhiteSpace(@params.Q)
                ? true
                : (m.FullName.Contains(@params.Q) ||
                   (m.Email != null && m.Email.Contains(@params.Q)) ||
                   (m.Phone != null && m.Phone.Contains(@params.Q)) ||
                   (m.Subject != null && m.Subject.Contains(@params.Q)) ||
                   m.MessageBody.Contains(@params.Q))
        )
    {
        if (!isCount)
        {
            AddOrderByDescending(m => m.CreatedAt);
            ApplyPagination(@params.PageSize, @params.Page);
        }
    }

    public MessagesSpecification(int id)
        : base(m => m.MessageId == id)
    {
    }
}
