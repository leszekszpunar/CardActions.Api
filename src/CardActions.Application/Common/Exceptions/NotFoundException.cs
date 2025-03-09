namespace CardActions.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string title, string message)
        : base(message)
    {
        Title = title;
    }

    public string Title { get; }
}