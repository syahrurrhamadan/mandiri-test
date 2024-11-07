using System.ComponentModel.DataAnnotations;

public class PermissionFormUpdate
{
    [Required]
    public string? name { get; set; }
    public string? description { get; set; }

}
