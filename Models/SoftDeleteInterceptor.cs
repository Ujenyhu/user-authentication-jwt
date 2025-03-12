using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using userauthjwt.DataAccess.Interfaces;

namespace userauthjwt.Models
{
    public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is null) return result;

            foreach(var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry is not { State: Microsoft.EntityFrameworkCore.EntityState.Deleted, Entity: ISoftDelete delete }) continue;

                entry.State = EntityState.Modified;
                delete.IsDeleted = true;
                delete.DeletedAt = DateTime.Now;
            }
            return result;
        }

    }
}
