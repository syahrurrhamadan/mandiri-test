using App.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebApi.Models;

namespace App.Authorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        // There can only be one policy provider in ASP.NET Core.
        // We only handle permissions related policies, for the rest
        /// we will use the default provider.
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    // Dynamically creates a policy with a requirement that contains the permission.
    // The policy name must match the permission that is needed.
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        // Console.WriteLine("[hanabi] log 1");
        // Console.WriteLine(policyName.StartsWith("/api", StringComparison.OrdinalIgnoreCase));
        // Console.WriteLine(policyName);

        if (policyName.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult(policy.Build());
        }

        // Policy is not for permissions, try the default provider.
        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
    {
        // Return a default policy if no specific policy is found
        return GetDefaultPolicyAsync();
    }
}
