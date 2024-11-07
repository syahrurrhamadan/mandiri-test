using System.ComponentModel.DataAnnotations;

public class PermissionFormCreate
{
    [Required]
    public string? name { get; set; }
    public string? description { get; set; }

}
