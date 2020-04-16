using AutoMapper;
using Mud.Container;
using Mud.Settings;

namespace Mud.Repository.Mongo.Common
{
    public abstract class RepositoryBase<T> : MongoRepository<T>
    {
        private const string DatabaseName = "MudNet";

        protected IMapper Mapper => DependencyContainer.Current.GetInstance<IMapper>();
        protected ISettings Settings => DependencyContainer.Current.GetInstance<ISettings>();

        protected RepositoryBase()
            :this(typeof(T).Name)
        {
        }

        protected RepositoryBase(string collectionName)
            :base(DependencyContainer.Current.GetInstance<ISettings>().ConnectionString, DatabaseName, collectionName)
        {
        }
    }
}
