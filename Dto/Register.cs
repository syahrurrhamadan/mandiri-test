using System.ComponentModel.DataAnnotations;
namespace WebApi.Dto;
public class RegisterModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
    public string? ConfirmPassword { get; set; }

    [Required]
    public string? Name { get; set; }
}
