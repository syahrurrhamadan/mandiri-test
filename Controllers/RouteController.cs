using System.Security.Claims;
using App.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/v1/route")]
    [Produces("application/json")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;
        public RouteController(DatabaseContext dbContext, ILogger<RouteController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("refresh")]
        [Authorize(Api.Route.Refresh)]
        public async Task<ActionResult<RouteMaster>> Refresh(List<string> urls)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                foreach (var url in urls)
                {
                    if (!_dbContext.RouteMaster.Any(at => at.Name == url))
                    {
                        // Create a new data
                        var newData = new RouteMaster
                        {
                            Name = url,
                            Alias = url,
                            Flag = "FE",
                        };

                        _dbContext.RouteMaster.Add(newData);
                        _dbContext.SaveChanges();
                    }
                }

                return JsonHelper.Content(new { success = "00" });
            }
            catch (Exception ex)
            {
                // Log error untuk pengembangan atau pengawasan
                // throw new Exception(ex.Message);
                _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                    _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Authorize(Api.Route.GetDatas)]
        public async Task<ActionResult<IEnumerable<RouteMaster>>> GetDatas()
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                // Memuat Permission & User saat mengambil data Roles
                var datas = await _dbContext.RouteMaster.ToListAsync();

                return JsonHelper.Content(new { success = "00", datas });
            }
            catch (Exception ex)
            {
                // Log error untuk pengembangan atau pengawasan
                // throw new Exception(ex.Message);
                _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                    _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Api.Route.GetData)]
        public async Task<ActionResult<RouteMaster>> GetData(Guid id)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var data = await _dbContext.RouteMaster
                    .Include(ad => ad.Permissions)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (data == null) return NotFound(new { success = "04", message = "Data not found" });

                return JsonHelper.Content(new { success = "00", data });
            }
            catch (Exception ex)
            {
                // Log error untuk pengembangan atau pengawasan
                // throw new Exception(ex.Message);
                _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                    _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet("access-validation")]
        [Authorize]
        public async Task<ActionResult<RouteMaster>> AccessValidation([FromQuery] string? name = null)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var validasi = false;
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return JsonHelper.Content(new { success = "00", validasi });
                }

                var superadmin = _dbContext.UserRole
                    .Join(
                        _dbContext.RoleMasters,
                        ur => ur.RoleId,
                        rm => rm.RoleId,
                        (ur, rm) => new { UserRole = ur, RoleMaster = rm }
                    )
                    .Where(x => x.UserRole.UserId == new Guid(userId))
                    .Select(x => new
                    {
                        x.RoleMaster.RoleName,
                    })
                    .FirstOrDefault();

                if (superadmin != null && superadmin.RoleName == "Superadmin")
                {
                    return JsonHelper.Content(new { success = "00", validasi = true });
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
                    .Where(x => x.UserRole.UserId == new Guid(userId) && x.Route.Name == name && x.Route.Flag == "FE")
                    .Select(x => new
                    {
                        x.Permission.Id,
                        x.Permission.Name,
                    })
                    .Any();

                if (data)
                {
                    validasi = true;
                }

                return JsonHelper.Content(new { success = "00", validasi });
            }
            catch (Exception ex)
            {
                // Log error untuk pengembangan atau pengawasan
                // throw new Exception(ex.Message);
                _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                    _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
                return StatusCode(500);
            }
        }
    }
}
