using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
  [Index(nameof(RoleName), IsUnique = true)]
  public class RoleMaster
  {
    [Key]
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public List<UserRole>? UserRoles { get; set; }
    public List<RoleHasPermission>? Permissions { get; set; }
  }
}