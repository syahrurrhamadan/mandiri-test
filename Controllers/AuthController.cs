using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApi.Services;
using HandlebarsDotNet;
using WebApi.Helpers;
using WebApi.Dto;
using Microsoft.Extensions.Options;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace WebApi.Controllers
{
    [Route("api/v1/auth")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly GeneralSettings _generalSettings;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EncryptionHelper _encyptHelper;

        public AuthController(DatabaseContext dbContext, ILogger<AuthController> logger, IOptions<JwtSettings> jwtSettings, IOptions<GeneralSettings> generalSettings, IHttpClientFactory httpClientFactory, EncryptionHelper encryptionHelper)
        {
            _dbContext = dbContext;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _generalSettings = generalSettings.Value;
            _httpClientFactory = httpClientFactory;
            _encyptHelper = encryptionHelper;

        }

        private string GenerateJwtToken(UserMaster user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                // Tambahkan klaim tambahan sesuai kebutuhan
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Audience,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: AppHelper.JakartaTime().AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class ApiResponse
        {
            public bool Status { get; set; }
            public int ErrorCode { get; set; }
            public string ErrorDescription { get; set; }
            public string Token { get; set; }
            public DateTime Validity { get; set; }
            public UserInfo UserInfo { get; set; }
            public List<Role> Roles { get; set; }
        }

        public class UserInfo
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
        }

        public class Role
        {
            public string RoleCode { get; set; }
            public string RoleName { get; set; }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserMaster>> Login([FromBody] Login model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Console.WriteLine("error message username: {0} ",model.Username);
                string usernameDecrypted = model.Username;
                string passwordDecrypted = model.Password;

                var data = _dbContext.UserMasters
                .Join(
                    _dbContext.UserRole,
                    um => um.Id,
                    ur => ur.UserId,
                    (um, ur) => new { UserMaster = um, UserRole = ur }
                )
                .Join(
                    _dbContext.RoleMasters,
                    combined => combined.UserRole.RoleId,
                    rm => rm.RoleId,
                    (combined, rm) => new { combined.UserMaster, combined.UserRole, RoleMasters = rm }
                )
                .Where(x => x.UserMaster.Username == usernameDecrypted)
                .Select(x => new
                {
                    x.RoleMasters.RoleName,
                    x.UserMaster,
                })
                .FirstOrDefault();

                if (data != null && data.RoleName == "Superadmin")
                {

                    if (!string.IsNullOrEmpty(data.UserMaster.PhotoPath))
                    {
                        try
                        {
                            // Asumsikan bahwa path relatif dimulai dari root aplikasi
                            string filePath = Path.Combine(Directory.GetCurrentDirectory(), data.UserMaster.PhotoPath);

                            if (System.IO.File.Exists(filePath))
                            {
                                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                                data.UserMaster.PhotoPath = Convert.ToBase64String(imageBytes);
                            }
                            else
                            {
                                data.UserMaster.PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                            }
                        }
                        catch (Exception)
                        {
                            // Log error untuk pengembangan atau pengawasan
                            data.UserMaster.PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                        }
                    }

                    if (!BCrypt.Net.BCrypt.Verify(passwordDecrypted, data.UserMaster.Password))
                    {
                        return Unauthorized(new { success = "01", error = "Invalid username or password" });
                    }

                    // get token
                    var superadminToken = GetToken(data.UserMaster);

                    _dbContext.SaveChanges();

                    return JsonHelper.Content(new { success = "00", token = superadminToken, user = data.UserMaster });
                }

                var user = _dbContext.UserMasters.FirstOrDefault(u => u.Username == usernameDecrypted);
                if (user == null)
                {
                    return Unauthorized(new { success = "01", error = "Invalid username or password" });
                }

                if (!string.IsNullOrEmpty(user.PhotoPath))
                {
                    try
                    {
                        // Asumsikan bahwa path relatif dimulai dari root aplikasi
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), user.PhotoPath);

                        if (System.IO.File.Exists(filePath))
                        {
                            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                            user.PhotoPath = Convert.ToBase64String(imageBytes);
                        }
                        else
                        {
                            user.PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                        }
                    }
                    catch (Exception)
                    {
                        // Log error untuk pengembangan atau pengawasan
                        user.PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                    }
                }

                if (!BCrypt.Net.BCrypt.Verify(passwordDecrypted, user.Password))
                {
                    return Unauthorized(new { success = "01", error = "Invalid username or password" });
                }

                // get Token
                var token = GetToken(user);

                _dbContext.SaveChanges();

                return JsonHelper.Content(new { success = "00", token, user });
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                    _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var data = await _dbContext.RefreshTokens
                    .Where(x => x.Token == token)
                    .ToListAsync();

                if (data == null) return NotFound(new { success = "04", message = "Data not found" });

                _dbContext.RefreshTokens.RemoveRange(data);
                await _dbContext.SaveChangesAsync();

                return JsonHelper.Content(new { success = "00" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                    _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
                return StatusCode(500);
            }
        }

        private string? GetToken(UserMaster user)
        {
            var token = GenerateJwtToken(user);

            var newToken = new RefreshToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            _dbContext.RefreshTokens.Add(newToken);

            return token;
        }

        [HttpGet("/error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment)
        {
            return Problem();
        }

        [HttpGet("/api/v1/healthcheck")]
        public IActionResult Healthcheck()
        {
            return Ok();
        }
    }
}
