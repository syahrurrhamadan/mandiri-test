namespace WebApi.Models
{
    public class PermissionHasRoute
    {
        public Guid? Id { get; set; }
        public string? RouteName { get; set; }
        public string? PermissionName { get; set; }
        public RouteMaster? Route { get; set; }
        public Permission? Permission { get; set; }
    }
}
