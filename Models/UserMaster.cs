using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
  [Index(nameof(Username), IsUnique = true)]
  public class UserMaster
  {
    [Key]
    public Guid? Id { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset? EmailVerifiedAt { get; set; }
    public string? Password { get; set; }
    public string? RememberToken { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdateAt { get; set; }
    public string? PhotoFilename { get; set; }
    public string? PhotoPath { get; set; }
    public string? VerificationToken { get; set; }
    public string? ResetToken { get; set; }
    public DateTimeOffset? ResetTokenExpires { get; set; }
    public DateTimeOffset? PasswordReset { get; set; }
    public string? Department { get; set; }
    public List<UserRole>? UserRoles { get; set; }
  }
}