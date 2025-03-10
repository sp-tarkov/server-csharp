using Microsoft.EntityFrameworkCore;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace CreateSqlLiteDb
{
    [Injectable]
    public class JsonToSqliteConverter(string dbPath)
    {
        public async Task ConvertObjectToSqliteAsync<T>(T objectToConvert) where T : class
        {
            await using (var db = new DataContext(dbPath))
            {
                await db.Database.EnsureCreatedAsync();
                db.Set<T>().Add(objectToConvert);
                await db.SaveChangesAsync();
            }
        }

        public async Task ConvertCollectionToSqliteAsync<T>(IList<T> objectsToConvert)
        {
            await using (var db = new DataContext(dbPath))
            {
                await db.Database.EnsureCreatedAsync();
                db.AddRange(objectsToConvert);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Tracks state
        /// </summary>
        public class DataContext(string dbPath) : DbContext
        {
            // We need each object we're going to import registered here :(
            public DbSet<HideoutArea> HideoutAreas
            {
                get; set;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
            {
                options.UseSqlite($"Data Source={dbPath}");
            }

            // set helper
            public void AddEntity<T>(T entity) where T : class
            {
                Set<T>().Add(entity);
            }

            // get helper
            public List<T> GetEntities<T>() where T : class
            {
                return Set<T>().ToList();
            }
        }
    }
}
