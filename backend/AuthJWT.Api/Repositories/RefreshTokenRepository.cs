using AuthJWT.Api.Data;
using AuthJWT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthJWT.Api.Repositories;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken> CreateAsync(RefreshToken token)
    {
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();
        return token;
    }

    public Task<RefreshToken?> GetByTokenAsync(string token) =>
        db.RefreshTokens
          .Include(t => t.User)
          .FirstOrDefaultAsync(t => t.Token == token);

    public async Task RevokeAsync(RefreshToken token)
    {
        token.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
