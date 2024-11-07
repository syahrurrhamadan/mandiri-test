using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
  public class Login
  {
    [Required]
    // [EmailAddress(ErrorMessage = "Invalid email address")]
    // public string? Email { get; set; }
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }
  }

}