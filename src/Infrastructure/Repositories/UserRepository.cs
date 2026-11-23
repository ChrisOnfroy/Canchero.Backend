using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementa las consultas y altas de perfiles de usuario.
/// </summary>
public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        => context.UserProfiles.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
        => context.UserProfiles.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await context.UserProfiles.AddAsync(user, cancellationToken);
}
