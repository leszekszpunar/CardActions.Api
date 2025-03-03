namespace CardActions.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public string Title { get; }

    public NotFoundException(string title, string message) 
        : base(message)
    {
        Title = title;
    }
} 