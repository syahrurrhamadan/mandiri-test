using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Models;

namespace App.Authorization;

class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{

    private readonly DatabaseContext _dbContext;
    private readonly ILogger _logger;

    public PermissionHandler(DatabaseContext dbContext, ILogger<PermissionHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        try
        {
            if (_dbContext == null) return Task.CompletedTask;

            if (!context.User.HasClaim(c =>
            {
                // Console.WriteLine(c.Type);
                return c.Type == ClaimTypes.NameIdentifier;
            }))
            {
                return Task.CompletedTask;
            }

            var userID = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userID == null)
            {
                return Task.CompletedTask;
            }

            var superadmin = _dbContext.UserRole
            .Join(
                _dbContext.RoleMasters,
                ur => ur.RoleId,
                rm => rm.RoleId,
                (ur, rm) => new { UserRole = ur, RoleMaster = rm }
            )
            .Where(x => x.UserRole.UserId == new Guid(userID))
            .Select(x => new
            {
                x.RoleMaster.RoleName,
            })
            .FirstOrDefault();

            if (superadmin != null && superadmin.RoleName == "Superadmin")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var data = _dbContext.Permission
                    .Join(
                        _dbContext.RoleHasPermission,
                        p => p.Name,
                        rp => rp.PermissionName,
                        (p, rp) => new { Permission = p, RolePermission = rp }
                    )
                    .Join(
                        _dbContext.UserRole,
                        combined => combined.RolePermission.RoleName,
                        ur => ur.Role.RoleName,
                        (combined, ur) => new { combined.Permission, combined.RolePermission, UserRole = ur }
                    )
                    .Join(
                        _dbContext.PermissionHasRoute,
                        combined => combined.Permission.Name,
                        ur => ur.PermissionName,
                        (combined, ur) => new { combined.Permission, combined.RolePermission, combined.UserRole, RoutePermission = ur }
                    )
                    .Join(
                        _dbContext.RouteMaster,
                        combined => combined.RoutePermission.RouteName,
                        ur => ur.Name,
                        (combined, ur) => new { combined.Permission, combined.RolePermission, combined.UserRole, combined.RoutePermission, Route = ur }
                    )
                    .Where(x => x.UserRole.UserId == new Guid(userID) && x.Route.Name == requirement.Permission && x.Route.Flag == "BE")
                    .Select(x => new
                    {
                        x.Permission.Id,
                        x.Permission.Name,
                    })
                    .Any();

            if (data)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log error untuk pengembangan atau pengawasan
            // throw new Exception(ex.Message);
            _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
            if (ex.InnerException != null)
                _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
            return Task.CompletedTask;
        }
    }
}
