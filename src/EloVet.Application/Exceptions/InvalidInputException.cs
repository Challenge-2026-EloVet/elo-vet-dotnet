namespace EloVet.Application.Exceptions;

public class InvalidInputException : EloVetException
{
    public InvalidInputException(string message)
        : base(message)
    {
    }
}
