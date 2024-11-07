using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Permission
    {
        [Key]
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? Description { get; set; }
        public List<RoleHasPermission>? Roles { get; set; }
        public List<PermissionHasRoute>? Routes { get; set; }
    }
}
