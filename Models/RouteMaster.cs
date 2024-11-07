using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(Flag))]
    public class RouteMaster
    {
        [Key]
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? Description { get; set; }
        public string? Flag { get; set; }
        public List<PermissionHasRoute>? Permissions { get; set; }
    }
}
