using WebApi.Models;
using WebApi.Helpers;
using WebApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Middlewares
{
  public class TokenValidationMiddleware
  {
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TokenValidationService tokenValidationService)
    {
      // Check if endpoint requires authorization
      var endpoint = context.GetEndpoint();
      if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
      {
        await _next(context);
        return;
      }

      if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"success\": \"02\",\"message\": \"Authorization header is missing.\"}");
        return;
      }

      var token = authorizationHeader.ToString().Replace("Bearer ", string.Empty);

      if (string.IsNullOrEmpty(token))
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"success\": \"02\",\"message\": \"Token is missing.\"}");
        return;
      }

      if (!await tokenValidationService.IsValidToken(token))
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"success\": \"02\",\"message\": \"Invalid token.\"}");
        return;
      }

      await _next(context);
    }
  }
}
