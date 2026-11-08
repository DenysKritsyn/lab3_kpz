using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace lab3_kpz.Core.WebApi.Authorization;

public sealed class OwnerOrAdminHandler
    : AuthorizationHandler<OwnerOrAdminRequirement, OwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdminRequirement requirement,
        OwnedResource resource)
    {
        if (context.User.IsInRole(AppRoles.Admin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (requirement.AllowDoctor && context.User.IsInRole(AppRoles.Doctor))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub");

        var userEmail = context.User.FindFirstValue(ClaimTypes.Email)
                        ?? context.User.FindFirstValue("email");

        if (!string.IsNullOrWhiteSpace(resource.OwnerId) &&
            (string.Equals(resource.OwnerId, userId, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(resource.OwnerId, userEmail, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

