using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace QueueApi.Infrastructure;

public static class RepositorySave
{
    public static async Task SaveChangesAsync(DbContext context)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            var postgres = FindPostgresException(exception);

            if (postgres is null)
                throw;

            throw postgres.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation =>
                    new ConflictException("A record with the same unique value already exists."),
                PostgresErrorCodes.ForeignKeyViolation =>
                    new ConflictException("The record is still in use or the related record does not exist."),
                _ => exception
            };
        }
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        Exception? current = exception;

        while (current is not null)
        {
            if (current is PostgresException postgres)
                return postgres;

            current = current.InnerException;
        }

        return null;
    }
}
