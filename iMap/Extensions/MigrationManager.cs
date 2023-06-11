using iMap.Data;
using Microsoft.EntityFrameworkCore;

namespace iMap.Extensions;

public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

                if (context.Database.ProviderName!="Microsoft.EntityFrameworkCore.InMemory")
                {
                    context.Database.Migrate();
                }
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        return host;
    }
}