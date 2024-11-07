namespace WebApi.Models
{
  public class UserRole
    {
        public Guid? Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? RoleId { get; set; }
        public UserMaster? User { get; set; }
        public RoleMaster? Role { get; set; }

    }
}