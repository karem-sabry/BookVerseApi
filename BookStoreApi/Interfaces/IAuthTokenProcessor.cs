namespace BookStoreApi.Interfaces;
using BookStoreApi.Entities;


public interface IAuthTokenProcessor
{
    public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user, IList<string> roles);
    public string GenerateRefreshToken();
}