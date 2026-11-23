using Application.Errors;
using Application.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Repositories;

/// <summary>
/// Confirma cambios de persistencia y traduce conflictos conocidos de PostgreSQL.
/// </summary>
public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgresException)
        {
            if (postgresException.SqlState == PostgresErrorCodes.ExclusionViolation)
            {
                throw new ConflictException("La cancha ya tiene una reserva en esa franja horaria.");
            }

            if (postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException("El email ya está registrado.");
            }

            throw;
        }
    }
}
