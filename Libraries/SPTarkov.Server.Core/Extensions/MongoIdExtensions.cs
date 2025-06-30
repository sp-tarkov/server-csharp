using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Extensions
{
    public static class MongoIdExtensions
    {
        //Temporary, but necessary
        public static IEnumerable<MongoId> ToMongoIds(this IEnumerable<string> source)
        {
            return source.Select(s => (MongoId)s);
        }
    }
}
