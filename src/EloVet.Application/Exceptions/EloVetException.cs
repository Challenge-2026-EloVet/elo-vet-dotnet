namespace EloVet.Application.Exceptions;

public abstract class EloVetException : Exception
{
    protected EloVetException(string message)
        : base(message)
    {
    }
}
