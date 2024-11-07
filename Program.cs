using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.Models;
using WebApi.Middlewares;
using WebApi.Dto;
using WebApi.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using App.Authorization;
using Serilog;
using System.Globalization;
using WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

// LOAD CONFIGURATION
var configurationBuilder = new ConfigurationBuilder();

configurationBuilder.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);

var configuration = configurationBuilder.Build();

// MAPP CONFIGURATION
builder.Services.Configure<GeneralSettings>(configuration.GetSection("GeneralSettings"));
builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
builder.Services.AddTransient<EncryptionHelper>();
// Read JWT settings from configuration
var jwtIssuer = configuration.GetSection("JwtSettings:Issuer").Get<string>();
var jwtAudience = configuration.GetSection("JwtSettings:Audience").Get<string>();
var jwtSecretKey = configuration.GetSection("JwtSettings:SecretKey").Get<string>();
// Read CORS settings from configuration
string? allowedOrigins = configuration.GetSection("CorsSettings").GetValue<string>("AllowedOrigins");

// SET LOGGER
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// CONNECT DATABASE
builder.Services.AddDbContext<DatabaseContext>(options => DBHelper.Postgres(configuration, options));

// SET AUTH => BEARER TOKEN
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

// SET PERMISSION 
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddTransient<IAuthorizationHandler, PermissionHandler>();

// SET TOKEN VALIDASI SERVICES
builder.Services.AddScoped<TokenValidationService>();

// SET HTTP CLIENT SERVICES
builder.Services.AddHttpClient();

// SET SWAGGER DOC
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo { Title = "Inscpecta Backend API", Version = "v1" });
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[]{}
            }
        });
        option.EnableAnnotations();
    }
);

// SET CORS POLICY
// define allowed domains, in this case "http://example.com" and "*" = all domains, for testing purposes only.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
        {
            // Note: The specified URL must not contain a trailing slash (/). 
            // If the URL terminates with /, the comparison returns false and no header is returned.
            builder.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});

// SET CONTROLLER SERVICES
builder.Services.AddControllers();

// MIDDLEWARE-ORDER
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-8.0#middleware-order
var app = builder.Build();


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler("/error");
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == StatusCodes.Status405MethodNotAllowed)
    {
        response.ContentType = "application/json";
        await response.WriteAsync("{\"error\": \"Method Not Allowed\"}");
    }
});


// Tambahkan middleware otentikasi dan otorisasi di sini
app.UseAuthentication();

app.UseMiddleware<TokenValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

try
{
    // Log.Information("Starting up");
    Console.WriteLine("Application start-up success");
    app.Run();
}
catch (Exception ex)
{
    // Log.Fatal(ex, "Application start-up failed");
    Console.WriteLine($"Application start-up failed {ex.Message}");
}
finally
{
    Log.CloseAndFlush();
}