namespace BusinessLogicLayer.Exceptions;

public class NotFoundExpection : Exception
{
    public NotFoundExpection(string name, object key) 
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
