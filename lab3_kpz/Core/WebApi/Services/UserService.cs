using System.Security.Claims;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.WebApi.Services;

public sealed class UserService(SymptomDiaryDbContext db) : IUserService
{
    public async Task<User> GetOrCreateCurrentUserAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirstValue("email")
                    ?? throw new UnauthorizedAccessException("Email claim not found in user access token.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.UserEmail == email, ct);
        if (user is not null)
        {
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

        return user;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == email, ct);
}

