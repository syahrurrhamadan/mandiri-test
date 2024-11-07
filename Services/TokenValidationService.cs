using WebApi.Models;
using WebApi.Helpers;

namespace WebApi.Services
{
  public class TokenValidationService
  {
    private readonly DatabaseContext _dbContext;

    public TokenValidationService(DatabaseContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<bool> IsValidToken(string token)
    {
      var tokenRecord = _dbContext.RefreshTokens.FirstOrDefault(t => t.Token == token);
      if (tokenRecord == null || tokenRecord.ExpiresAt < DateTime.SpecifyKind(AppHelper.JakartaTime(), DateTimeKind.Utc))
      {
        return false;
      }
      return true;
    }
  }
}
