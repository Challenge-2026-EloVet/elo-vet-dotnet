namespace EloVet.Application.Exceptions;

public class ConflictException : EloVetException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
