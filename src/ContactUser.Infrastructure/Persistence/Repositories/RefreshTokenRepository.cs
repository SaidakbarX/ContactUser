using ContactUser.Application.Interfaces;
using ContactUser.Core.Errors;
using ContactUser.Domain.Entities;
using ContactUser.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContactUser.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext _mainContext) : IRefreshTokenRepository
{
    public async Task AddRefreshToken(RefreshToken refreshToken)
    {
        await _mainContext.RefreshTokens.AddAsync(refreshToken);
        await _mainContext.SaveChangesAsync();
    }

    public async Task DeleteRefreshToken(string refreshToken)
    {
        var token = _mainContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token == null)
        {
            throw new EntityNotFoundException();
        }

    }

    public async Task<RefreshToken> SelectRefreshToken(string refreshToken, long userId) => await _mainContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
}
