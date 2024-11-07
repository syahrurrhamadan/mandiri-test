using System.ComponentModel.DataAnnotations;

public class RolesFormCreate
{
    [Required]
    public Guid? user_id { get; set; }
    [Required]
    public Guid[]? role_id { get; set; }

}
