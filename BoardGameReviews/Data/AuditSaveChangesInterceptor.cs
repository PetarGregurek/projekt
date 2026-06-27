using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BoardGameReviews.Data;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;

    public AuditSaveChangesInterceptor(ILogger<AuditSaveChangesInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        LogChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        LogChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _logger.LogError(eventData.Exception, "Error saving changes to the database.");
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogError(eventData.Exception, "Error saving changes to the database.");
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void LogChanges(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var entityName = entry.Metadata.ClrType.Name;
            var key = GetPrimaryKey(entry);

            _logger.LogInformation(
                "Database {Action} for {Entity} with key {EntityKey}",
                entry.State,
                entityName,
                key);
        }
    }

    private static string GetPrimaryKey(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();

        if (primaryKey == null || primaryKey.Properties.Count == 0)
        {
            return "(no primary key)";
        }

        var values = primaryKey.Properties
            .Select(property =>
            {
                var value = entry.Property(property.Name).CurrentValue;
                return $"{property.Name}={value ?? "(null)"}";
            });

        return string.Join(", ", values);
    }
}
