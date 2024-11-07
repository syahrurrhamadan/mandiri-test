namespace WebApi.Models
{
  public class RefreshToken
    {
       public Guid?  RefreshTokenId { get; set; }
        public Guid? UserId { get; set; }
        public string? Token { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }

    }
}