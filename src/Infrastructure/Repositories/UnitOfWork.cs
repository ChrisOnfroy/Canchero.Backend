using Application.Errors;
using Application.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Infrastructure.Repositories;

/// <summary>
/// Confirma cambios de persistencia y traduce conflictos conocidos de PostgreSQL.
/// </summary>
public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? transaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (transaction is null) throw new InvalidOperationException("No hay transacción activa.");
        await transaction.CommitAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (transaction is null) return;
        await transaction.RollbackAsync(cancellationToken);
        await transaction.DisposeAsync();
        transaction = null;
    }

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
