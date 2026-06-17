using backend.Entities;

namespace backend.Services;

public interface ITokenService
{
    Task<(string AccessToken, DateTime ExpiresAt)> CreateAccessTokenAsync(AppUser user);
    (string RawToken, string HashedToken) CreateRefreshToken();
    string HashToken(string rawToken);
}
