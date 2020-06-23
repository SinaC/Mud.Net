using AutoMapper;
using Mud.Settings;

namespace Mud.Repository.Mongo.Common
{
    public abstract class RepositoryBase<T> : MongoRepository<T>
    {
        private const string DatabaseName = "MudNet";

        protected IMapper Mapper { get; }
        protected ISettings Settings { get; }

        protected RepositoryBase(IMapper mapper, ISettings settings)
            : this(mapper, settings, typeof(T).Name)
        {
        }

        protected RepositoryBase(IMapper mapper, ISettings settings, string collectionName)
            : base(settings.ConnectionString, DatabaseName, collectionName)
        {
            Mapper = mapper;
            Settings = settings;
        }
    }
}
