using System.ComponentModel.DataAnnotations;

public class RoutePermissionFormCreate
{
    [Required]
    public string? permission_name { get; set; }
    [Required]
    public string[]? route_name { get; set; }

}
