using System.ComponentModel.DataAnnotations;
using FileUploadApi.Validation;
namespace WebApi.Dto;
public class UserFormCreate
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
    public string? ConfirmPassword { get; set; }

    public string? Name { get; set; }

    public string? Username { get; set; }

    [PngFile]
    public IFormFile? Photo { get; set; }
    public Guid[]? RoleId { get; set; }
    public string? Department { get; set; }
}
