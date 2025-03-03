namespace CardActions.Infrastructure.Data.Entities;

public class CardAction
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<CardActionRule> Rules { get; set; } = new List<CardActionRule>();
} 