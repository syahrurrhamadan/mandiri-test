using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Dto;
using App.Authorization;
using WebApi.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.Controllers
{
    [Route("api/v1/users")]
    [Produces("application/json")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;

        public UsersController(DatabaseContext dbContext, IWebHostEnvironment webHostEnvironment, ILogger<UsersController> logger)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // [HttpPost]
        // [Authorize(Api.Users.AddData)]
        // public async Task<IActionResult> AddData([FromForm] UserFormCreate model)
        // {
        //     // _logger.LogInformation("[POST] User Create.");
        //     try
        //     {

        //         if (!ModelState.IsValid)
        //         {
        //             return BadRequest(ModelState);
        //         }

        //         // Check if the email is already taken
        //         if (_dbContext.UserMasters.Any(u => u.Email == model.Email))
        //         {
        //             ModelState.AddModelError("Email", "Email is already taken.");
        //             return BadRequest(ModelState);
        //         }

        //         // Hash the password
        //         string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

        //         // Create a new user
        //         var newUser = new UserMaster
        //         {
        //             Email = model.Email,
        //             Password = hashedPassword,
        //             Name = model.Name,
        //             Username = model.Username,
        //             Department = model.Department,
        //         };

        //         // Handle file upload
        //         // if (model.Photo != null)
        //         // {
        //         //     // Generate a unique filename
        //         //     string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;

        //         //     string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");

        //         //     // Ensure the Uploads folder exists
        //         //     Directory.CreateDirectory(uploadFolder);

        //         //     // Combine the path with the wwwroot path
        //         //     string filePath = Path.Combine(uploadFolder, uniqueFileName);

        //         //     // Save the file to the server
        //         //     using (var stream = new FileStream(filePath, FileMode.Create))
        //         //     {
        //         //         await model.Photo.CopyToAsync(stream);
        //         //     }

        //         //     // Get base URL
        //         //     var baseUri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";

        //         //     // Update the user's photo information
        //         //     newUser.PhotoFilename = uniqueFileName;
        //         //     newUser.PhotoPath = $"{baseUri}/Uploads/{uniqueFileName}";
        //         // }

        //         // Save the new user to the database
        //         _dbContext.UserMasters.Add(newUser);

        //         if (model.RoleId != null)
        //         {
        //             foreach (var item in model.RoleId)
        //             {
        //                 var newData = new UserRole
        //                 {
        //                     UserId = newUser.Id,
        //                     RoleId = item,
        //                 };

        //                 _dbContext.UserRole.Add(newData);
        //             }
        //         }

        //         newUser.CreatedAt = DateTime.SpecifyKind(AppHelper.JakartaTime(), DateTimeKind.Utc);
        //         newUser.UpdateAt = DateTime.SpecifyKind(AppHelper.JakartaTime(), DateTimeKind.Utc);

        //         await _dbContext.SaveChangesAsync();

        //         return JsonHelper.Content(new { success = "00", User = newUser });
        //     }
        //     catch (Exception ex)
        //     {
        //         // Log error untuk pengembangan atau pengawasan
        //         // throw new Exception(ex.Message);
        //         _logger.LogError("Exception {Source} {Message} {StackTrace}", ex.Source, ex.Message, ex.StackTrace);
        //         if (ex.InnerException != null)
        //             _logger.LogError("InnerException {Source} {Message} {StackTrace}", ex.InnerException.Source, ex.InnerException.Message, ex.InnerException.StackTrace);
        //         return StatusCode(500);
        //     }
        // }

        [HttpGet]
        [Authorize(Api.Users.GetDatas)]
        public async Task<ActionResult<IEnumerable<UserMaster>>> GetDatas()
        {
            try
            {
                if (_dbContext == null)
                {
                    return NotFound(new { success = "04", message = "Data not found" });
                }
                var users = await _dbContext.UserMasters
                    .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                    .ToListAsync();

                for (int i = 0; i < users.Count; i++)
                {
                    if (!string.IsNullOrEmpty(users[i].PhotoPath))
                    {
                        try
                        {
                            // Asumsikan bahwa path relatif dimulai dari root aplikasi
                            string filePath = Path.Combine(Directory.GetCurrentDirectory(), users[i].PhotoPath);

                            if (System.IO.File.Exists(filePath))
                            {
                                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                                users[i].PhotoPath = Convert.ToBase64String(imageBytes);
                            }
                            else
                            {
                                users[i].PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                            }
                        }
                        catch (Exception)
                        {
                            // Log error untuk pengembangan atau pengawasan
                            users[i].PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                        }
                    }
                }

                return JsonHelper.Content(new { success = "00", users });
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
        [Authorize]
        public async Task<ActionResult<UserMaster>> GetData(Guid id)
        {
            try
            {
                if (_dbContext == null)
                {
                    return NotFound(new { success = "04", message = "Data not found" });
                }

                var user = await _dbContext.UserMasters
                    .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (user == null)
                {
                    return NotFound(new { success = "04", message = "Data not found" });
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
                return JsonHelper.Content(new { success = "00", user });
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
        [Authorize(Api.Users.UpdateData)]
        public async Task<IActionResult> Data(Guid id, [FromForm] UserFormUpdate model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _dbContext.UserMasters.FirstOrDefaultAsync(a => a.Id == id);

                if (existingUser == null)
                {
                    return NotFound(new { success = "04", message = "Data not found" });
                }

                // Update properties based on the model
                existingUser.Email = model.Email;
                // Only update the password if it is provided
                // if (!string.IsNullOrEmpty(model.Password))
                // {
                //     // Hash the password
                //     string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                //     existingUser.Password = hashedPassword;
                // }
                existingUser.Name = model.Name;
                // existingUser.Username = model.Username;
                // existingUser.Department = model.Department;

                // Handle file upload
                if (model.Photo != null)
                {
                    // Generate a unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + ".png";

                    string uploadFolder = Path.Combine("files", "Uploads");

                    // Ensure the Uploads folder exists
                    Directory.CreateDirectory(uploadFolder);

                    // Combine the path with the wwwroot path
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var stream = new MemoryStream())
                    {
                        await model.Photo.CopyToAsync(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        using (Image<Rgba32> image = Image.Load<Rgba32>(stream))
                        {
                            image.Save(filePath, new PngEncoder());
                        }
                    }
                    // Get base URL
                    // var baseUri = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";
                    // var baseUri = $"https://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";

                    // // Update the user's photo information
                    existingUser.PhotoFilename = uniqueFileName;
                    existingUser.PhotoPath = filePath;
                }

                if (model.Scheme != "profile")
                {
                    // reset roles
                    var resetUserRoles = await _dbContext.UserRole.Where(e => e.UserId == existingUser.Id).ToListAsync();
                    if (resetUserRoles.Any()) _dbContext.UserRole.RemoveRange(resetUserRoles);

                    // reassign
                    if (model.RoleId != null)
                    {
                        foreach (var item in model.RoleId)
                        {
                            var newData = new UserRole
                            {
                                UserId = existingUser.Id,
                                RoleId = item,
                            };

                            _dbContext.UserRole.Add(newData);
                        }
                    }
                }

                existingUser.UpdateAt = DateTime.SpecifyKind(AppHelper.JakartaTime(), DateTimeKind.Utc);

                await _dbContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(existingUser.PhotoPath))
                {
                    try
                    {
                        // Asumsikan bahwa path relatif dimulai dari root aplikasi
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), existingUser.PhotoPath);

                        if (System.IO.File.Exists(filePath))
                        {
                            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                            existingUser.PhotoPath = Convert.ToBase64String(imageBytes);
                        }
                        else
                        {
                            existingUser.PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                        }
                    }
                    catch (Exception)
                    {
                        // Log error untuk pengembangan atau pengawasan
                        existingUser.PhotoPath = null; // atau Anda dapat mengatur pesan error atau handling lain
                    }
                }

                return JsonHelper.Content(new { success = "00", User = existingUser });
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
        [Authorize(Api.Users.DeleteData)]
        public async Task<IActionResult> DeleteData(Guid id)
        {
            try
            {
                var user = await _dbContext.UserMasters.FirstOrDefaultAsync(a => a.Id == id);
                if (user == null) return NotFound(new { success = "04", message = "Data not found" });
                _dbContext.UserMasters.Remove(user);
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

        [HttpGet("role-current/{id}")]
        [Authorize(Api.Users.RoleCurrent)]
        public async Task<ActionResult<UserMaster>> RoleCurrent(Guid id)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });

                var assigned = await _dbContext.RoleMasters
                .Join(
                    _dbContext.UserRole,
                    p => p.RoleId,
                    r => r.RoleId,
                    (p, r) => new { RoleMaster = p, UserRole = r }
                )
                .Where(x => x.UserRole.UserId == id)
                .Select(x => new { id = x.RoleMaster.RoleId, name = x.RoleMaster.RoleName })
                .ToListAsync();
                if (assigned == null)
                {
                    return NotFound(new { success = "04", message = "Data not found" });
                }

                var available = await _dbContext.RoleMasters
                .Select(x => new { id = x.RoleId, name = x.RoleName })
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

        [HttpPost("role-assign")]
        [Authorize(Api.Users.RoleAssign)]
        public async Task<ActionResult<UserMaster>> RoleAssign([FromBody] RolesFormCreate model)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });


                if (!ModelState.IsValid || model.role_id == null)
                {
                    return BadRequest(ModelState);
                }

                // INIT TASK GROUP
                // List<Task> tasks = new List<Task>();

                foreach (var item in model.role_id)
                {
                    // CREATE AND START A NEW TASK FOR EACH USER
                    // tasks.Add(Task.Run(async () =>
                    // {
                    if (_dbContext.RoleMasters.Any(a => a.RoleId == item) && _dbContext.UserMasters.Any(a => a.Id == model.user_id) && !_dbContext.UserRole.Any(a => a.UserId == model.user_id && a.RoleId == item))
                    {
                        // Create a new data
                        var newData = new UserRole
                        {
                            UserId = model.user_id,
                            RoleId = item,
                        };

                        _dbContext.UserRole.Add(newData);
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

        [HttpPost("role-remove")]
        [Authorize(Api.Users.RoleRemove)]
        public async Task<ActionResult<UserMaster>> RoleRemove([FromBody] RolesFormCreate model)
        {
            try
            {
                if (_dbContext == null) return NotFound(new { success = "01", message = "cant connect to your database" });


                if (!ModelState.IsValid || model.role_id == null)
                {
                    return BadRequest(ModelState);
                }

                // INIT TASK GROUP
                // List<Task> tasks = new List<Task>();

                foreach (var item in model.role_id)
                {
                    // CREATE AND START A NEW TASK FOR EACH USER
                    // tasks.Add(Task.Run(async () =>
                    // {
                    if (_dbContext.RoleMasters.Any(a => a.RoleId == item) && _dbContext.UserMasters.Any(a => a.Id == model.user_id))
                    {
                        var data = await _dbContext.UserRole
                        .FirstOrDefaultAsync(a => a.UserId == model.user_id && a.RoleId == item);
                        if (data != null)
                        {
                            _dbContext.UserRole.Remove(data);
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
    }
}
