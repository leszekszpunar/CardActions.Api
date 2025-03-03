using CardActions.Domain.Models;

namespace CardActions.Application.Services;

public interface ICardService
{
    Task<CardDetails?> GetCardDetails(string userId, string cardNumber);
} 