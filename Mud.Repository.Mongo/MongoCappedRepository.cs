using MongoDB.Bson;
using MongoDB.Driver;

namespace Mud.Repository.Mongo
{
    public class MongoCappedRepository<TDocument> : MongoRepository<TDocument>
    {
        private const long DefaultMaxSize = 16 * 1024 * 1024; // 16 Mb

        public long MaxDocuments { get; }
        public long MaxSize { get; }

        public MongoCappedRepository(string connectionString, string databaseName, string collectionName, long maxDocuments) : this(connectionString, databaseName, collectionName, maxDocuments, DefaultMaxSize)
        {
        }

        public MongoCappedRepository(string connectionString, string databaseName, string collectionName, long maxDocuments, long maxSize) : base(connectionString, databaseName, collectionName)
        {
            MaxDocuments = maxDocuments;
            MaxSize = maxSize;

            // Create capped collection if it doesn't exist
            if (!CollectionExists(Db, CollectionName))
            {
                Db.CreateCollection(CollectionName, new CreateCollectionOptions
                {
                    Capped = true,
                    MaxDocuments = maxDocuments,
                    MaxSize = maxSize
                });
            }
        }

        public bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            return database.ListCollections(new ListCollectionsOptions
            {
                Filter = new BsonDocument("name", collectionName)
            }).Any();
        }
    }
}
