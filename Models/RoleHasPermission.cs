namespace WebApi.Models
{
    public class RoleHasPermission
    {
        public Guid? Id { get; set; }
        public string? RoleName { get; set; }
        public string? PermissionName { get; set; }
        public RoleMaster? Role { get; set; }
        public Permission? Permission { get; set; }
    }
}
