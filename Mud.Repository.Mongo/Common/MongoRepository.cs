using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mud.Repository.Mongo.Common
{
    public class MongoRepository<TDocument> : MongoContext
    {
        protected IMongoDatabase Db { get; }

        public string CollectionName { get; }

        public IMongoCollection<TDocument> Collection => Db.GetCollection<TDocument>(CollectionName);

        public MongoRepository(string connectionString, string databaseName, string collectionName)
        {
            CollectionName = collectionName;

            //MongoClient client;
            //var connectionString = PPCConfigurationManager.MongoConnectionString;
            //if (string.IsNullOrWhiteSpace(connectionString))
            //    client = new MongoClient();
            //else
            //    client = new MongoClient(connectionString);

            var mongoConnectionUrl = new MongoUrl(connectionString);
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);
            mongoClientSettings.ClusterConfigurator = cb => {
                cb.Subscribe<CommandStartedEvent>(e => {
                    if (e.CommandName != "isMaster" && e.CommandName != "buildInfo" && e.CommandName != "getLastError" && e.CommandName != "saslStart" && e.CommandName != "saslContinue")
                        Debug.Print($"MONGO CMD {e.CommandName} - {e.Command.ToJson()}");
                });
            };
            MongoClient client = new MongoClient(mongoClientSettings);

            Db = client.GetDatabase(databaseName);
        }

        public virtual IEnumerable<TDocument> GetAll()
        {
            return Collection.Find(Builders<TDocument>.Filter.Empty).ToList();
        }

        public virtual async Task<IEnumerable<TDocument>> GetAllAsync(CancellationToken cancellationToken)
        {
            var results = await Collection.FindAsync(Builders<TDocument>.Filter.Empty, cancellationToken: cancellationToken);
            return await results.ToListAsync(cancellationToken);
        }

        public virtual void Insert(TDocument document)
        {
            Collection.InsertOne(document);
        }

        public virtual async Task InsertAsync(TDocument document, CancellationToken cancellationToken)
        {
            await Collection.InsertOneAsync(document, new InsertOneOptions { BypassDocumentValidation = true }, cancellationToken);
        }

        public virtual void Fetch(IEnumerable<TDocument> documents)
        {
            Db.DropCollection(CollectionName);
            Collection.InsertMany(documents);
        }

        public virtual async Task FetchAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken)
        {
            await Db.DropCollectionAsync(CollectionName, cancellationToken);
            await Collection.InsertManyAsync(documents, new InsertManyOptions { BypassDocumentValidation = true, IsOrdered = false }, cancellationToken);
        }
    }
}
