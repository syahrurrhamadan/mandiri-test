using System.ComponentModel.DataAnnotations;

public class PermissionFormAssignRemove
{
    [Required]
    public string? role_id { get; set; }
    [Required]
    public string[]? permission_id { get; set; }

}
