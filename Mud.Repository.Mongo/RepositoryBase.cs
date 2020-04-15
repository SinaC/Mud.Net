using AutoMapper;
using Mud.Container;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Repository.Mongo
{
    public abstract class RepositoryBase<T>
    {
        private const string DatabaseName = "MudNet";

        protected IMapper Mapper => DependencyContainer.Instance.GetInstance<IMapper>();
        protected ISettings Settings => DependencyContainer.Instance.GetInstance<ISettings>();

        public string CollectionName { get; }

        public MongoRepository<T> MongoRepository { get; }

        public RepositoryBase(string collectionName)
        {
            CollectionName = collectionName;

            string connectionString = Settings.ConnectionString;
            MongoRepository = new MongoRepository<T>(connectionString, DatabaseName, CollectionName);
        }
    }
}
