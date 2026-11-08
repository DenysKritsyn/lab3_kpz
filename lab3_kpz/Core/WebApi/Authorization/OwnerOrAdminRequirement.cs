using Microsoft.AspNetCore.Authorization;

namespace lab3_kpz.Core.WebApi.Authorization;

public sealed class OwnerOrAdminRequirement(bool allowDoctor = false) : IAuthorizationRequirement
{
    public bool AllowDoctor { get; } = allowDoctor;
}

public sealed record OwnedResource(string OwnerId);

