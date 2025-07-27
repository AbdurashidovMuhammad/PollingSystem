using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Polling.DataAccess.Persistence;

public static class AutomatedMigration
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();

        if (context.Database.IsNpgsql())
            await context.Database.MigrateAsync();
    }
}
