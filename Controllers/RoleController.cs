using System.Security.Claims;
using App.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Dto;
using Microsoft.Data.SqlClient;

namespace WebApi.Controllers
{
    [Route("api/v1/role")]
    [Produces("application/json")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;
        public RoleController(DatabaseContext dbContext, ILogger<RoleController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Api.Role.AddData)]
        public async Task<ActionResult<RoleMaster>> AddData(RoleMasterFormCreate formCreate)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                // CREATE A NEW DATA
                var newData = new RoleMaster
                {
                    RoleName = formCreate.RoleName,
                };

                _dbContext.RoleMasters.Add(newData);
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
        [Authorize(Api.Role.GetDatas)]
        public async Task<ActionResult<IEnumerable<RoleMaster>>> GetDatas()
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                // Memuat Permission & User saat mengambil data Roles
                var datas = await _dbContext.RoleMasters.Include(a => a.UserRoles).Include(ad => ad.Permissions).ToListAsync();

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
        [Authorize(Api.Role.GetData)]
        public async Task<ActionResult<RoleMaster>> GetData(Guid id)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var data = await _dbContext.RoleMasters
                    .Include(a => a.UserRoles)
                    .Include(ad => ad.Permissions)
                    .FirstOrDefaultAsync(a => a.RoleId == id);

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
        [Authorize(Api.Role.UpdateData)]
        public async Task<ActionResult<RoleMaster>> UpdateData(Guid id, RoleMasterFormUpdate formUpdate)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var data = await _dbContext.RoleMasters
                    .Include(a => a.UserRoles)
                    .Include(ad => ad.Permissions)
                    .FirstOrDefaultAsync(a => a.RoleId == id);

                if (data == null) return NotFound(new { success = "04", message = "Data not found" });

                var newRoleName = formUpdate.RoleName;
                var oldRoleName = data.RoleName;

                // Ambil entitas RoleHasPermission yang terkait sebelum dihapus
                var rolePermissions = await _dbContext.RoleHasPermission
                    .Where(rp => rp.RoleName == oldRoleName)
                    .ToListAsync();

                // Hapus entitas RoleHasPermission yang terkait
                _dbContext.RoleHasPermission.RemoveRange(rolePermissions);
                await _dbContext.SaveChangesAsync();

                // Perbarui RoleName menggunakan SQL mentah
                var updateCommand = "UPDATE RoleMasters SET RoleName = @newRoleName WHERE RoleId = @roleId";
                await _dbContext.Database.ExecuteSqlRawAsync(updateCommand,
                    new SqlParameter("@newRoleName", newRoleName),
                    new SqlParameter("@roleId", id));
                await _dbContext.SaveChangesAsync();

                // Tambahkan kembali entitas RoleHasPermission dengan RoleName baru
                List<RoleHasPermission> RoleHasPermissions = [];
                foreach (var rolePermission in rolePermissions)
                {
                    var newRoleHasPermission = new RoleHasPermission
                    {
                        RoleName = newRoleName,
                        PermissionName = rolePermission.PermissionName,
                    };
                    RoleHasPermissions.Add(newRoleHasPermission);
                }
                _dbContext.RoleHasPermission.AddRange(RoleHasPermissions);
                await _dbContext.SaveChangesAsync();

                // Ambil kembali data yang telah diperbarui
                data = await _dbContext.RoleMasters
                    .Include(a => a.UserRoles)
                    .Include(ad => ad.Permissions)
                    .FirstOrDefaultAsync(a => a.RoleId == id);

                return JsonHelper.Content(new { success = "00", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = "99", message = ex.Message });
            }

        }

        [HttpDelete("{id}")]
        [Authorize(Api.Role.DeleteData)]
        public async Task<IActionResult> DeleteData(Guid id)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var data = await _dbContext.RoleMasters.FindAsync(id);

                if (data == null) return NotFound(new { success = "04", message = "Data not found" });

                var oldRoleName = data.RoleName;
                // Ambil entitas RoleHasPermission yang terkait sebelum dihapus
                var rolePermissions = await _dbContext.RoleHasPermission
                    .Where(rp => rp.RoleName == oldRoleName)
                    .ToListAsync();

                // Hapus entitas RoleHasPermission yang terkait
                _dbContext.RoleHasPermission.RemoveRange(rolePermissions);
                await _dbContext.SaveChangesAsync();


                _dbContext.RoleMasters.Remove(data);
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
