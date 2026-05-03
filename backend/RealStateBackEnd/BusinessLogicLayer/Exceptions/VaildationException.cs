using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
namespace BusinessLogicLayer.Exceptions;

public sealed class VaildationException : Exception
{
    public IEnumerable<string> Errors { get; set; } = [];

    public VaildationException(IEnumerable<string> errors) : base("One or more validation errors have occurred.")
    {
        Errors = errors;
    }
}

