using MediatR;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Data;

namespace ECommerce.API.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ApplicationDbContext _db;

    public TransactionBehavior(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip creating a new transaction if one is already active
        if (_db.Database.CurrentTransaction != null)
        {
            return await next();
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            // Centralized tracking: Automatically saves changes before committing
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return response;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}