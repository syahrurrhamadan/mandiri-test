using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using App.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/v1/permission")]
    [Produces("application/json")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;
        public PermissionController(DatabaseContext dbContext, ILogger<PermissionController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Api.Permission.AddData)]
        public async Task<ActionResult<Permission>> AddData(PermissionFormCreate formCreate)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var checker = await _dbContext.Permission
                    .FirstOrDefaultAsync(a => a.Name == formCreate.name);

                if (checker != null)
                {
                    ModelState.AddModelError("name", "name must be unique.");
                    return BadRequest(ModelState);
                }

                // Create a new data
                var newData = new Permission
                {
                    Name = formCreate.name,
                    Alias = formCreate.name,
                    Description = formCreate.description,
                };

                _dbContext.Permission.Add(newData);
                await _dbContext.SaveChangesAsync();

                return JsonHelper.Content(new { success = "00", data = newData });
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
        [Authorize(Api.Permission.GetDatas)]
        public async Task<ActionResult<IEnumerable<Permission>>> GetDatas()
        {
            try
            {
                if (_dbContext == null)
                {
                    return NotFound(new { success = "01", message = "cant connect to your database" });
                }
                var permissions = await _dbContext.Permission.ToListAsync();
                return JsonHelper.Content(new { success = "00", permissions });
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
        [Authorize(Api.Permission.GetData)]
        public async Task<ActionResult<IEnumerable<Permission>>> GetData(Guid id)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var data = await _dbContext.Permission
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

        [HttpPut("{id}")]
        [Authorize(Api.Permission.UpdateData)]
        public async Task<ActionResult<Permission>> UpdateData(Guid id, PermissionFormUpdate formUpdate)
        {
            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var data = await _dbContext.Permission
                    .FirstOrDefaultAsync(a => a.Id == id);
                if (data == null) return NotFound(new { success = "04", message = "Data not found" });

                data.Name = formUpdate.name;
                data.Alias = formUpdate.name;
                data.Description = formUpdate.description;

                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();

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

        [HttpDelete("{id}")]
        [Authorize(Api.Permission.DeleteData)]
        public async Task<IActionResult> DeleteData(Guid id)
        {
            try
            {
                var permission = await _dbContext.Permission.FindAsync(id);
                if (permission == null) return NotFound(new { success = "04", message = "Data not found" });

                var roleHasPermissions = await _dbContext.RoleHasPermission.Where(e => e.PermissionName == permission.Name).ToListAsync();
                var permissionHasRoutes = await _dbContext.PermissionHasRoute.Where(e => e.PermissionName == permission.Name).ToListAsync();

                if (permissionHasRoutes.Any()) _dbContext.PermissionHasRoute.RemoveRange(permissionHasRoutes);

                if (roleHasPermissions.Any()) _dbContext.RoleHasPermission.RemoveRange(roleHasPermissions);

                _dbContext.Permission.Remove(permission);
                await _dbContext.SaveChangesAsync();

                return Ok(new { success = "00" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting data");
                if (ex.InnerException != null)
                {
                    _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
                }
                return StatusCode(500, new { success = "01", message = "An error occurred while processing your request" });
            }
        }


        [HttpGet("refresh")]
        [Authorize(Api.Permission.Refresh)]
        public async Task<ActionResult<Permission>> Refresh()
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                string GetControllerName(string typeName)
                {
                    // Assuming the controller name is always in the format "XYZController"
                    if (typeName.EndsWith("Controller"))
                    {
                        return typeName.Substring(0, typeName.Length - "Controller".Length);
                    }
                    return typeName;
                }

                string GetPolicyName(MethodInfo methodInfo)
                {
                    var authorizeAttributes = methodInfo.GetCustomAttributes<AuthorizeAttribute>();
                    if (authorizeAttributes.Any())
                    {
                        return string.Join(",", authorizeAttributes.Select(a => a.Policy));
                    }
                    return "-";
                }

                var controllerActionList = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
                    .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                    .Where(m => !m.GetCustomAttributes<ApiExplorerSettingsAttribute>().Any())
                    .Select(x => new
                    {
                        Controller = GetControllerName(x.DeclaringType.Name),
                        Action = x.Name,
                        // ReturnType = x.ReturnType.Name,
                        // Attributes = string.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", ""))),
                        Policy = GetPolicyName(x)
                    })
                    .OrderBy(x => x.Controller).ThenBy(x => x.Action)
                    .ToList();

                controllerActionList.ForEach(i =>
                {
                    if (!i.Policy.Equals("-") && i.Policy != null && i.Policy != "")
                    {
                        if (!_dbContext.RouteMaster.Any(a => a.Name == i.Policy))
                        {
                            // Create a new data
                            var newData = new RouteMaster
                            {
                                Name = i.Policy,
                                Alias = i.Controller + " " + i.Action,
                                Flag = "BE",
                            };

                            _dbContext.RouteMaster.Add(newData);
                            _dbContext.SaveChanges();
                        }
                    }
                });

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

        [HttpGet("current/{name}")]
        [Authorize(Api.Permission.Current)]
        public async Task<ActionResult<Permission>> RoleCurrent(string name)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var assigned = await _dbContext.Permission
                .Join(
                    _dbContext.RoleHasPermission,
                    p => p.Name,
                    r => r.PermissionName,
                    (p, r) => new { Permission = p, RolePermission = r }
                )
                .Where(x => x.RolePermission.RoleName == name)
                .Select(x => new { id = x.Permission.Id, name = x.Permission.Name })
                .ToListAsync();
                if (assigned == null)
                {
                    return NotFound(new { success = "04", message = "Data not found" });
                }

                var available = await _dbContext.Permission
                .Select(x => new { id = x.Id, name = x.Name })
                .ToListAsync();
                if (available == null)
                {
                    return NotFound(new { success = "04", message = "Data not found" });
                }

                assigned.ForEach(i =>
                {
                    var itemToRemove = available.SingleOrDefault(r => r.id == i.id);
                    if (itemToRemove != null)
                    {
                        available.Remove(itemToRemove);
                    }
                });

                return JsonHelper.Content(new { success = "00", assigned, available });
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

        [HttpPost("assign")]
        [Authorize(Api.Permission.Assign)]
        public async Task<ActionResult<Permission>> RoleAssign([FromBody] PermissionFormAssignRemove model)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });


                if (!ModelState.IsValid || model.permission_id == null)
                {
                    return BadRequest(ModelState);
                }

                // INIT TASK GROUP
                // List<Task> tasks = new List<Task>();

                foreach (var item in model.permission_id)
                {
                    // CREATE AND START A NEW TASK FOR EACH USER
                    // tasks.Add(Task.Run(async () =>
                    // {
                    if (_dbContext.Permission.Any(a => a.Name == item) && _dbContext.RoleMasters.Any(a => a.RoleName == model.role_id) && !_dbContext.RoleHasPermission.Any(a => a.RoleName == model.role_id && a.PermissionName == item))
                    {
                        // Create a new data
                        var newData = new RoleHasPermission
                        {
                            RoleName = model.role_id,
                            PermissionName = item,
                        };

                        _dbContext.RoleHasPermission.Add(newData);
                    }
                    // }));
                }

                // WAIT FOR ALL TASKS TO COMPLETE
                // await Task.WhenAll(tasks);

                await _dbContext.SaveChangesAsync();

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

        [HttpPost("remove")]
        [Authorize(Api.Permission.Remove)]
        public async Task<ActionResult<Permission>> RoleRemove([FromBody] PermissionFormAssignRemove model)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });


                if (!ModelState.IsValid || model.permission_id == null)
                {
                    return BadRequest(ModelState);
                }

                // INIT TASK GROUP
                // List<Task> tasks = new List<Task>();

                foreach (var item in model.permission_id)
                {
                    // CREATE AND START A NEW TASK FOR EACH USER
                    // tasks.Add(Task.Run(async () =>
                    // {
                    if (_dbContext.Permission.Any(a => a.Name == item) && _dbContext.RoleMasters.Any(a => a.RoleName == model.role_id))
                    {
                        var data = await _dbContext.RoleHasPermission
                        .FirstOrDefaultAsync(a => a.RoleName == model.role_id && a.PermissionName == item);
                        if (data != null)
                        {
                            _dbContext.RoleHasPermission.Remove(data);
                        }
                    }
                    // }));
                }

                // WAIT FOR ALL TASKS TO COMPLETE
                // await Task.WhenAll(tasks);

                await _dbContext.SaveChangesAsync();

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

        [HttpGet("route-current/{id}")]
        [Authorize(Api.Permission.RouteCurrent)]
        public async Task<ActionResult<Permission>> RouteCurrent(Guid id)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var assigned = await _dbContext.RouteMaster
                    .Join(
                        _dbContext.PermissionHasRoute,
                        p => p.Name,
                        r => r.RouteName,
                        (p, r) => new { RouteMaster = p, PermissionHasRoute = r }
                    )
                    .Join(
                        _dbContext.Permission,
                        combined => combined.PermissionHasRoute.PermissionName,
                        r => r.Name,
                        (combined, ur) => new { combined.RouteMaster, combined.PermissionHasRoute, Permission = ur }
                    )
                    .Where(x => x.Permission.Id == id)
                    .Select(x => new { id = x.RouteMaster.Id, name = x.RouteMaster.Name })
                    .ToListAsync();

                if (assigned == null) return NotFound(new { success = "04", message = "Data not found" });

                var available = await _dbContext.RouteMaster
                    .Select(x => new { id = x.Id, name = x.Name })
                    .ToListAsync();

                if (available == null) return NotFound(new { success = "04", message = "Data not found" });

                assigned.ForEach(i =>
                {
                    var itemToRemove = available.SingleOrDefault(r => r.name == i.name);
                    if (itemToRemove != null)
                    {
                        available.Remove(itemToRemove);
                    }
                });

                return JsonHelper.Content(new { success = "00", assigned, available });
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

        [HttpPost("route-assign")]
        [Authorize(Api.Permission.RouteAssign)]
        public async Task<ActionResult<Permission>> RouteAssign([FromBody] RoutePermissionFormCreate model)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });


                if (!ModelState.IsValid || model.route_name == null)
                {
                    return BadRequest(ModelState);
                }

                foreach (var item in model.route_name)
                {
                    if (_dbContext.Permission.Any(a => a.Name == model.permission_name) && _dbContext.RouteMaster.Any(a => a.Name == item) && !_dbContext.PermissionHasRoute.Any(a => a.RouteName == item && a.PermissionName == model.permission_name))
                    {
                        // Create a new data
                        var newData = new PermissionHasRoute
                        {
                            RouteName = item,
                            PermissionName = model.permission_name,
                        };

                        _dbContext.PermissionHasRoute.Add(newData);
                    }
                }

                // WAIT FOR ALL TASKS TO COMPLETE
                await _dbContext.SaveChangesAsync();

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

        [HttpPost("route-remove")]
        [Authorize(Api.Permission.RouteRemove)]
        public async Task<ActionResult<Permission>> RouteRemove([FromBody] RoutePermissionFormCreate model)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });


                if (!ModelState.IsValid || model.route_name == null)
                {
                    return BadRequest(ModelState);
                }

                foreach (var item in model.route_name)
                {
                    // CREATE AND START A NEW TASK FOR EACH USER
                    if (_dbContext.Permission.Any(a => a.Name == model.permission_name) && _dbContext.RouteMaster.Any(a => a.Name == item))
                    {
                        var data = await _dbContext.PermissionHasRoute
                        .FirstOrDefaultAsync(a => a.RouteName == item && a.PermissionName == model.permission_name);
                        if (data != null)
                        {
                            _dbContext.PermissionHasRoute.Remove(data);
                        }
                    }
                }

                // WAIT FOR ALL TASKS TO COMPLETE
                await _dbContext.SaveChangesAsync();

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
    }
}
