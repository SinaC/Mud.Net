using AutoMapper;
using Mud.Container;
using Mud.Settings;

namespace Mud.Repository.Mongo.Common
{
    public abstract class RepositoryBase<T>
    {
        private const string DatabaseName = "MudNet";

        protected IMapper Mapper => DependencyContainer.Instance.GetInstance<IMapper>();
        protected ISettings Settings => DependencyContainer.Instance.GetInstance<ISettings>();

        public string CollectionName { get; }

        public MongoRepository<T> MongoRepository { get; }

        protected RepositoryBase(string collectionName)
        {
            CollectionName = collectionName;
            MongoRepository = new MongoRepository<T>(Settings.ConnectionString, DatabaseName, CollectionName);
        }
    }
}
