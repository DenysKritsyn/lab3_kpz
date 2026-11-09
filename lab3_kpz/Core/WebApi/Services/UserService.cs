using System.Security.Claims;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace lab3_kpz.Core.WebApi.Services;

public sealed class UserService(
    SymptomDiaryDbContext db,
    IMemoryCache cache) : IUserService
{
    public async Task<User> GetOrCreateCurrentUserAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirstValue("email")
                    ?? throw new UnauthorizedAccessException("Email claim not found in user access token.");

        var cacheKey = $"user:email:{email.ToLowerInvariant()}";
        if (cache.TryGetValue<User>(cacheKey, out var cachedUser) && cachedUser is not null)
        {
            return cachedUser;
        }

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == email, ct);
        if (user is not null)
        {
            cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
            return user;
        }

        var name = principal.FindFirstValue("name")
                   ?? principal.FindFirstValue("preferred_username")
                   ?? email;

        user = new User
        {
            UserEmail = email,
            UserName = name
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
        return user;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == email, ct);
}
